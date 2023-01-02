( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Zilog Z80 CPU Assembler                                                )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - romfile.fs                                                       )
(           - audio.fs                                                         )
(                                                                              )
( ---------------------------------------------------------------------------- )
Z80 definitions   Glossary Assembler definitions {

( ---------------------------------------------------------------------------- )
(       Errors                                                                 )
( ---------------------------------------------------------------------------- )
: unknown-error ( flag -- ) abort" Unknown error" ;
: invalid-error ( flag -- ) abort" Invalid instruction" ;
: address-error ( flag -- ) abort" Address out of range" ;
:     imm-error ( flag -- ) abort" Immediate value out of range" ;
:    disp-error ( flag -- ) abort" Displacement out of range" ;
:   accum-error ( flag -- ) abort" Illegal register (must be A)" ;
:      hl-error ( flag -- ) abort" Illegal register (must be HL, IX, or IY)" ;
:      sp-error ( flag -- ) abort" Illegal register (must be SP)" ;
:   index-error ( flag -- ) abort" Illegal register (IX and IY not allowed)" ;
:    jump-error ( flag -- ) abort" Destination out of range" ;
:      cc-error ( flag -- ) abort" Condition code required" ;
:     /cc-error ( flag -- ) abort" Illegal condition (must be Z or C bit)" ;

: invalid ( -- ) true invalid-error ;

( ---------------------------------------------------------------------------- )
(       Registers                                                              )
( ---------------------------------------------------------------------------- )
$10000 make b       $10004 make h       $20000 make bc      $30000 make [bc]
$10001 make c       $10005 make l       $20001 make de      $30001 make [de]
$10002 make d       $10007 make a       $20002 make hl      $30002 make [hl]
$10003 make e       $20007 make af      $20003 make sp      $40007 make  i
:  =a?   ( reg -- )  a   <> accum-error ;                   $4000F make  r
:  =hl?  ( reg -- )  hl  <>    hl-error ;
: =[hl]? ( reg -- ) [hl] <>    hl-error ;
:  =sp?  ( reg -- )  sp  <>    sp-error ;              
: <>sp?  ( reg -- reg ) dup sp = invalid-error ;     
: <>af?  ( reg -- reg ) dup af = invalid-error ;

( ---------------------------------------------------------------------------- )
(       Index Registers                                                        )
( ---------------------------------------------------------------------------- )
0 value prefix
: /index  ( -- ) prefix index-error ;
: prefix, ( -- ) prefix if prefix c, endif  0 to prefix ;
: ix ( -- n ) $DD to prefix  hl ;
: iy ( -- n ) $FD to prefix  hl ;

1000 value disp
: !disp ( n -- ) ?schar not disp-error to disp ;
:  disp,  ( -- ) disp 1000 <> if disp c, endif  1000 to disp ;
: [#ix] ( n1 -- n2 ) !disp  $DD to prefix  [hl] ;
: [#iy] ( n1 -- n2 ) !disp  $FD to prefix  [hl] ;
:  [ix]    ( -- n )  0 [#ix] ;
:  [iy]    ( -- n )  0 [#iy] ;
: index? ( -- flag ) disp 1000 <> ;

( ---------------------------------------------------------------------------- )
(       Condition Codes                                                        )
( ---------------------------------------------------------------------------- )
$50000 make z<>     $50002 make cclr    $50004 make odd     $50006 make pos
$50001 make z=      $50003 make cset    $50005 make even    $50007 make neg
:  cc  ( n -- n' ) dup z<> neg  1+ within not  cc-error 7 and 3 lshift ;
: /cc? ( n -- f  )     z<> cset 1+ within ;
: /cc  ( n -- n' ) dup /cc? not /cc-error 7 and 3 lshift ;

( ---------------------------------------------------------------------------- )
(       Immediate Values                                                       )
( ---------------------------------------------------------------------------- )
$60000 make #       $70000 make [#]

:   #, ( n -- ) ?char  not imm-error         c, ;
:  u#, ( n -- ) ?uchar not address-error     c, ;
:  ##, ( n -- ) ?half  not imm-error     >2< h, ;
: u##, ( n -- ) ?uhalf not address-error >2< h, ;

( ---------------------------------------------------------------------------- )
(       Operand Processors                                                     )
( ---------------------------------------------------------------------------- )
$90001 make  r'     $90002 make  p'     $90003 make  m'     $90006 make  #'
$90011 make rr'     $90022 make pp'     $90013 make rm'     $90014 make ri'
$90031 make mr'     $90041 make ir'
$90061 make #r'     $90062 make #p'     $90063 make #m'
$90071 make @r'     $90017 make r@'     $90072 make @p'     $90027 make p@'

: 1arg    ( n1 -- n1 n' )         dup   16 rshift $90000 + ;
: 2arg ( n1 n2 -- n1 n2 n' ) 1arg third 12 rshift + ;

( ---------------------------------------------------------------------------- )
(       Operations                                                             )
( ---------------------------------------------------------------------------- )
: reg   ( reg -- n ) 7 and 3 lshift ;
: reg'  ( reg -- n ) 7 and ;
: sdreg ( sreg dreg -- n ) reg swap reg' + ;
: ireg  ( reg -- n ) 15 and ;
: preg  ( reg -- n ) 3 and 4 lshift ;
: mreg  ( reg -- n ) 1 and 4 lshift ;
: bit#  ( n1 -- n2 ) dup 0 8 within not imm-error 3 lshift ;

: op,   ( opcode -- ) prefix, c, disp, ;
: op2,  ( opcode -- ) prefix, h, disp, ;
: hlop, ( op hlop preg ) dup hl = if drop op, drop exit endif nip preg + op2, ;

( ---------------------------------------------------------------------------- )
: load, ( src dest -- )
    dup [#] = if
        drop swap 1arg case
            r' of =a? $32 op, u##, endof
            p' of <>af? $ED43 $22 rot hlop, u##, endof
        invalid endcase
    exit endif
    2arg case
        rr' of sdreg $40 + op, endof
        #r' of nip reg $06 + op, #, endof
        #m' of =[hl]? drop $36 op, #, endof
        #p' of nip preg $01 + op, ##, endof
        mr' of over [hl] = if nip reg  $46 else =a? mreg $0A endif + op, endof
        rm' of tuck [hl] = if nip reg' $70 else =a? mreg $02 endif + op, endof
        @r' of =a? drop $3A op, u##, endof
        @p' of nip <>af? $ED4B $2A rot hlop, u##, endof
        pp' of =sp? =hl? $F9 op, endof
        ir' of      =a? ireg $ED50 + op2, endof
        ri' of swap =a? ireg $ED40 + op2, endof
    invalid endcase ;

: push, ( preg -- ) 1arg p' <> invalid-error <>sp? preg $C5 + op, ;
: pull, ( preg -- ) 1arg p' <> invalid-error <>sp? preg $C1 + op, ;

: math8: ( opcode "name" -- ) create hostc,
    does> c@ >r 1arg case
        r' of reg' r> + op, endof
        #' of drop $46 r> + op, #, endof
        m' of =[hl]? $06 r> + op, endof
    invalid endcase ;
$A0 math8: and,     $A8 math8: xor,     $80 math8: add8,    $88 math8: addc8,
$B0 math8:  or,     $B8 math8: compare, $90 math8: sub,     $98 math8: subc8,   

: add,    1arg p' = if =hl? <>af?        preg   $09 + op,  exit endif add8,  ;
: addc,   1arg p' = if =hl? <>af? /index preg $ED4A + op2, exit endif addc8, ;
: subc,   1arg p' = if =hl? <>af? /index preg $ED42 + op2, exit endif subc8, ;

: inc: ( opcode "name" ) create hostc,
    does> c@ >r 1arg case
        r' of reg r> + op, endof
        m' of =[hl]? $30 r> + op, endof
        p' of preg r> $04 = if $03 else $0B endif + op, endof
    invalid endcase ;
$04 inc: inc,           $05 inc: dec,

: shift: ( opcode "name" -- ) create hostc,
    does> c@ >r 1arg case
        r' of reg' r>  over 7 = over $20 < and
              if + op, else + $CB00 + op2, endif endof
        m' of =[hl]? $CB op, $06 r> + c, endof
    invalid endcase ;
$20 shift: lsl,     $28 shift: asr,     $00 shift: rol,     $10 shift: rolc, 
$38 shift: lsr,                         $08 shift: ror,     $18 shift: rorc,

: bit: ( opcode "name" -- ) create hostc,
    does> c@ >r 2arg case
        #r' of reg' nip swap bit# + $CB00 + r> + op2, endof
        #m' of =[hl]? $CB op, drop bit# $06 + r> + c, endof
    invalid endcase ;
$40 bit: test-bit,      $80 bit: clear-bit,     $C0 bit: set-bit,

:  #a>in,  ( port# -- ) $DB op, u#, ;
:  #a<out, ( port# -- ) $D3 op, u#, ;
: [c]>in,    ( reg -- ) reg $ED40 + op2, ;    aka [bc]>in,
: [c]<out,   ( reg -- ) reg $ED41 + op2, ;    aka [bc]<out, 

: op ( opcode "name" -- ) create hostc,   does>  c@ op, ;
$27 op decimal,     $3F op invert-carry,    $00 op nop,     $F3 op -interrupts,
$2F op invert,      $37 op set-carry,       $76 op halt,    $FB op +interrupts,
$C9 op return,      $D9 op exchange,
$08 op af><af,      $EB op de><hl, aka hl><de,  $E3 op [sp]><hl, aka hl><[sp],

: 2op ( opcode "name" -- ) create host,   does>  @ op2, ;
$ED44 2op negate,   $ED46 2op imode0,   $ED56 2op imode1,   $ED5E 2op imode2,
$ED4D 2op ireturn,  $ED45 2op nreturn,
$EDA0 2op +load,    $EDB0 2op +copy,    $EDA1 2op +compare, $EDB1 2op +search,
$EDA8 2op -load,    $EDB8 2op -copy,    $EDA9 2op -compare, $EDB9 2op -search,
$EDA2 2op +in,      $EDB2 2op +read,    $EDA3 2op +out,     $EDB3 2op +write,
$EDAA 2op -in,      $EDBA 2op -read,    $EDAB 2op -out,     $EDBB 2op -write,
$DDE3 2op [sp]><ix, aka ix><[sp],       $FDE3 2op [sp]><iy, aka iy><[sp],

: jump, ( addr -- )
    dup [hl] <> if $C3 op, u##, exit endif drop
    index? if disp invalid-error 1000 to disp endif $E9 op, ;
: call,   ( addr -- ) $CD op, u##, ;
: jump?,  ( addr cc ) cc $C2 + op, u##, ;
: call?,  ( addr cc ) cc $C4 + op, u##, ;
: +jump,  ( disp -- ) ?schar not disp-error $18 op, c, ;
: +jump?, ( disp cc ) /cc $20 + op, ?schar not disp-error c, ;
: return?,  ( cc -- ) cc $C0 + op, ;
: restart,   ( addr ) dup -57 and address-error $C7 + op, ;
: dec-jump,  ( disp ) ?schar not disp-error $10 op, c, ;

( ---------------------------------------------------------------------------- )
(       Macro Instructions                                                     )
( ---------------------------------------------------------------------------- )
: clear, ( reg -- )
    1arg case
        r' of dup a = if xor, else 0 # rot load, endif endof
        p' of <>af? 0 # rot load, endof
    invalid endcase ;

: splitpair  ( preg -- reghi reglo ) <>sp? <>af? 3 and 2* $10000 + dup 1+ ;
: load-pair, ( preg1 preg2 -- ) splitpair rot splitpair rot load, swap load, ;

: lsl-pair, ( preg -- ) splitpair lsl, rolc, ;
: lsr-pair, ( preg -- ) splitpair swap lsr, rorc, ;
: asr-pair, ( preg -- ) splitpair swap asr, rorc, ;

( ---------------------------------------------------------------------------- )
(       Flow Control                                                           )
( ---------------------------------------------------------------------------- )
: displacement  ( n -- n' ) romspace - 2 - ;
: displacement> ( n -- n' ) romspace swap - 2 - ;

: begin    ( -- dest ) romspace ;
: again    ( dest -- ) displacement +jump, ;
: until ( dest cc -- ) 1 xor >r displacement r> +jump?, ;

: if ( cc -- cc orig ) 1 xor romspace 0 h, ;
: then  ( cc orig -- )
    dup displacement> ?schar not disp-error  $FF and rot
    ?dup if /cc $20 + else $18 endif 8 lshift + swap romh! ;   aka endif

: ahead  ( -- cc orig )              0 romspace 0 h, ;
: else   ( cc orig -- cc orig )      } ahead { 2swap } then { ;
: while  ( dest cc -- cc orig dest ) } if { rot ;
: repeat ( cc orig dest -- )         } again then { ;

: for ( count -- dest ) # b load, } begin { ;
: loop ( dest -- ) displacement dec-jump, ;

( ---------------------------------------------------------------------------- )
(       Callers                                                                )
( ---------------------------------------------------------------------------- )
: primitive, ( addr -- )
    dup displacement ?schar if +jump, drop exit endif drop jump, ;
: primitive?, ( addr cc -- )
    over displacement ?schar
    if over /cc? if swap +jump?, drop exit endif endif drop jump?, ;

synonym subroutine,  call,
synonym subroutine?, call?,

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















