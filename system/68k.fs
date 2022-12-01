( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Motorola 68000 CPU Assembler                                           )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - romfile.fs                                                       )
(                                                                              )
(       TODO:                                                                  )
(           - PC-relative addressing                                           )
(           - more testing                                                     )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions       Glossary Assembler68k definitions {

( ---------------------------------------------------------------------------- )
(       Errors                                                                 )
( ---------------------------------------------------------------------------- )
: unknown-error ( flag -- ) abort" Unknown error" ;
: invalid-error ( flag -- ) abort" Invalid instruction" ;
: address-error ( flag -- ) abort" Invalid address" ;
:  opsize-error ( flag -- ) abort" Illegal operation size" ;
:     imm-error ( flag -- ) abort" Immediate value out of range" ;
:  effect-error ( flag -- ) abort" Illegal effective address" ;
:    disp-error ( flag -- ) abort" Displacement out of range" ;
:    jump-error ( flag -- ) abort" Destination out of range" ;
:      cc-error ( flag -- ) abort" Condition code required" ;
:  number-error ( flag -- ) abort" Invalid number" ;

: invalid ( -- ) true invalid-error ;

( ---------------------------------------------------------------------------- )
(       Registers                                                              )
( ---------------------------------------------------------------------------- )
$1000 make tos  $1004 make d4   $2008 make np   $200C make fp   $3001 make ccr
$1001 make d1   $1005 make d5   $2009 make a1   $200D make tp   $3002 make sr
$1002 make d2   $1006 make d6   $200A make a2   $200E make sp   $3003 make usp
$1003 make d3   $1007 make d7   $200B make a3   $200F make rp

FloatStack not [IF] synonym a4 fp [THEN]

( ---------------------------------------------------------------------------- )
(       Condition Codes                                                        )
( ---------------------------------------------------------------------------- )
$4000 make yes                      $4800 make vclr
$4100 make no                       $4900 make vset
$4200 make ugt                      $4A00 make nclr aka pos
$4300 make ult=                     $4B00 make nset aka neg
$4400 make cclr aka ugt=            $4C00 make gt=
$4500 make cset aka ult             $4D00 make lt
$4600 make z<>                      $4E00 make gt
$4700 make z=                       $4F00 make lt=
: +cc ( cc n -- n' ) swap $FFFFF0FF demux $4000 <> cc-error  + ;

( ---------------------------------------------------------------------------- )
(       Operation Sizes                                                        )
( ---------------------------------------------------------------------------- )
variable opsize                                 : b  1 opsize ! ;   aka c
                                                : w  2 opsize ! ;   aka h
: nobyte ( -- ) opsize @ 1 = opsize-error ;
: noword ( -- ) opsize @ 2 = opsize-error ;
: nolong ( -- ) opsize @ 4 = opsize-error ;
: <byte> ( -- ) depth 0= if exit endif opsize @ 2 =  opsize-error  opsize off ;
: <word> ( -- ) depth 0= if exit endif opsize @ 1 =  opsize-error  opsize off ;
: <long> ( -- ) depth 0= if exit endif opsize @ 4 <> opsize-error  opsize off ;

: opsize+ ( n -- n' ) opsize @ 2/ 6 lshift + ;
: opmode+ ( n -- n' ) opsize @ nobyte  3 or 6 lshift + ;

( ---------------------------------------------------------------------------- )
(       Immediate Data                                                         )
( ---------------------------------------------------------------------------- )
$5000 make #

: imm#, ( n -- )
    opsize @ case
        1 of ?char not imm-error $FF and h, endof
        2 of ?half not imm-error         h, endof
        4 of ?cell not imm-error          , endof
    -1 opsize-error endcase ;

( ---------------------------------------------------------------------------- )
(       Effective Addresses                                                    )
( ---------------------------------------------------------------------------- )
variable ea         variable ea'
variable ext1       variable ext2       variable ext

: addr! ( n -- ) ea dup @ if drop ea' ext2 ext ! endif ! ;
: addr: ( n "name" -- ) create host, does> ( -- arg ) @ addr! $8000 ;
%010000 addr: [np]  %011000 addr: [np]+  %100000 addr: -[np]  %101000 addr: [np
%010001 addr: [a1]  %011001 addr: [a1]+  %100001 addr: -[a1]  %101001 addr: [a1
%010010 addr: [a2]  %011010 addr: [a2]+  %100010 addr: -[a2]  %101010 addr: [a2
%010011 addr: [a3]  %011011 addr: [a3]+  %100011 addr: -[a3]  %101011 addr: [a3
%010101 addr: [tp]  %011101 addr: [tp]+  %100101 addr: -[tp]  %101101 addr: [tp
%010110 addr: [sp]  %011110 addr: [sp]+  %100110 addr: -[sp]  %101110 addr: [sp
%010111 addr: [rp]  %011111 addr: [rp]+  %100111 addr: -[rp]  %101111 addr: [rp

FloatStack [IF]
%010100 addr: [fp]  %011100 addr: [fp]+  %100100 addr: -[fp]  %101100 addr: [fp
[ELSE]
%010100 addr: [a4]  %011100 addr: [a4]+  %100100 addr: -[a4]  %101100 addr: [a4
[THEN]

: (ndx) ( addr -- flag ) dup @ $38 and $28 <> dup if nip exit endif $8 rot +! ;    
:  ndx: ( n "name" -- ) create host, does> ( -- )
    ea (ndx) if ea' (ndx) effect-error endif @ ext @ +! ;
$080F ndx: tos+     $480F ndx: d4+      $880F ndx: np+    ( $C80F ndx: fp+ )
$180F ndx: d1+      $580F ndx: d5+      $980F ndx: a1+      $D80F ndx: tp+
$280F ndx: d2+      $680F ndx: d6+      $A80F ndx: a2+      $E80F ndx: sp+
$380F ndx: d3+      $780F ndx: d7+      $B80F ndx: a3+      $F80F ndx: rp+

$C80F FloatStack [IF] make fp+ [ELSE] make a4+ [THEN]

: ih ( -- ) ext @ dup @ $F7FF and swap ! ;   aka iw

: [#] ( n -- ) ?shalf not 1 and %111000 + addr!  ext @ !  $8000 ;
:  +] ( n -- )
    ext @ dup >r @ ?dup if
        $FF00 and swap ?schar not disp-error $FF and + r> !
    else ?shalf not disp-error $FFFF and r> ! endif ;
: 0] ( -- ) 0 +] ;

: <ext> ( ext ea n -- ext ea ) third @ swap 1 = if , exit endif h, ;
: (ext) ( ext ea -- )
    dup @ $38 and case
        $28 of over @ h, endof
        $30 of over @ h, endof
        $38 of dup @ 7 and dup 4 < if <ext> else drop endif endof               
    endcase 2drop ;
: ext1, ( -- ) ext1 ea  (ext) ;
: ext2, ( -- ) ext2 ea' (ext) ;

( ---------------------------------------------------------------------------- )
(       Data Field Address Register                                            )
( ---------------------------------------------------------------------------- )
synonym dfa  a3         synonym [dfa] [a3]      synonym -[dfa] -[a3]
synonym dfa+ a3+        synonym [dfa  [a3       synonym  [dfa]+ [a3]+

( ---------------------------------------------------------------------------- )
(       Operand Processors                                                     )
( ---------------------------------------------------------------------------- )
$9001 make  d'      $9002 make  a'      $9008 make  m'      $9031 make sd'
$9011 make dd'      $9012 make da'      $9018 make dm'      $9013 make ds'
$9021 make ad'      $9022 make aa'      $9028 make am'      $9083 make ms'
$9051 make #d'      $9052 make #a'      $9058 make #m'      $9053 make #s'
$9081 make md'      $9082 make ma'      $9088 make mm'      $9038 make sm'

: 1arg    ( n1 -- n1 n' )         dup  12 rshift $9000 + ;
: 2arg ( n1 n2 -- n1 n2 n' ) 1arg third 8 rshift + ;

( ---------------------------------------------------------------------------- )
(       Operations                                                             )
( ---------------------------------------------------------------------------- )
:  --?  ( -- flag )  ea @ -8 and $20 =   ea' @ -8 and $20 =   and ;
: areg  ( reg -- n ) 15 and ;
: sreg  ( reg -- n )  7 and ;
: dreg  ( reg -- n )  7 and 9 lshift ;
: sdreg ( sreg dreg -- n ) dreg swap sreg + ;
: adreg ( areg dreg -- n ) dreg swap areg + ;
:  ea,  ( n -- ) ea @ + opsize+ h, ext1, ;
: imm,  ( n opcode -- ) %111100 + h, imm#, ;
: clean ( -- ) ext1 off ext2 off ext1 ext !  ea off ea' off  4 opsize ! ; clean

( ---------------------------------------------------------------------------- )
: (easd)   ( -- n ) ea @ ea' @ sdreg ;
: (string) ( arg1 arg2 addr -- )
    @ >r 2arg case
        dd' of sdreg r> + opsize+ h, endof
        mm' of 2drop --? not effect-error  (easd)  r> + 8 + h, endof
    invalid endcase clean ;
:  bcd: ( opcode "name" -- ) create host, does> ( arg1 arg2 ) <byte> (string) ;
: addx: ( opcode "name" -- ) create host, does> ( arg1 arg2 ) (string) ;
$C100 bcd: abcd,    $8100 bcd: sbcd,    $D100 addx: addx,   $9100 addx: subx,

: (single) ( n opcode -- ) >r 1arg case
        d' of sreg r> + opsize+ h, endof
        m' of drop r> ea, endof
    invalid endcase clean ;
: set?, ( arg cc -- ) <byte> $50C0 +cc (single) ;
: nbcd,    ( arg -- ) <byte> $4800     (single) ;
: test,    ( arg -- )
    1arg a' = if 15 and $4A00 + opsize+ h, clean exit endif $4A00 (single) ;
: single: ( opcode "name" -- ) create host, does> ( arg -- ) @ (single) ;
$4000 single: negx,     $4400 single: neg,      $4600 single: not,

: (and1) ( n opcode -- n' ) $F100 and + ;
: (and2) ( n opcode -- n' ) $0F00 and + ;
: (data) ( arg1 arg2 opcode -- )
    >r 2arg case
        dd' of sdreg r> (and1) opsize+ h, endof
        dm' of drop dreg $100 + r> (and1) ea, endof
        #d' of  nip sreg r> (and2) opsize+ h, imm#, endof
        #m' of 2drop r> $0F00 and ea @ + opsize+ h, imm#, ext1, endof
    invalid endcase clean ;
: <status>  case ccr of noword b endof sr of nobyte w endof invalid endcase ;
: (status) ( arg1 arg2 opcode -- )
    >r 2arg #s' <> if r> (data) exit endif
    nip <status> 0 r> (and2) opsize+ imm, clean ;
: xor, ( arg1 arg2 -- ) $B000 (status) ;            aka eor,

: logic: ( opcode "name" -- ) create host, does> ( arg1 arg2 -- )
    @ >r 2arg md' = if nip dreg r> (and1) ea, clean exit endif r> (status) ;
$C200 logic: and,       $8000 logic: or,

: (quick) ( n arg opcode -- )
    >r 1arg case
        d' of swap sdreg r> (and1) opsize+ h, endof
        a' of swap adreg r> (and1) opsize+ h, endof
        m' of drop  dreg r> (and1) ea, endof
    invalid endcase clean ;
: arithmetic: ( opcodes "name" -- ) create swap host, host, does> ( arg1 arg2 )
    third } # { =
    if fourth 1 9 within if rot drop cell+ @ (quick) exit endif endif
    @ >r 2arg case
        da' of nobyte sdreg r> (and1) opmode+ h, endof
        ad' of nobyte adreg r> (and1) opsize+ h, endof
        aa' of nobyte adreg r> (and1) opmode+ $4 + h, endof
        md' of    nip  dreg r> (and1) ea, endof
        #a' of nobyte nip dreg r> (and1) opmode+ imm, endof
        ma' of nobyte nip dreg r> (and1) opmode+ ea @ + h, ext1, endof
        drop r> (data) exit
    endcase clean ;
$D600 $5000 arithmetic: add,        $9401 $5100 arithmetic: sub,

: shift: ( opcode "name" -- ) create host, does> ( arg1 arg2 -- )
    @ 3 demux third $8000 = if <byte> 9 lshift + nip $C0 + ea, else
    3 lshift + >r 2arg case
        dd' of swap sdreg $20 + r> + opsize+ h, endof
        #d' of nip over 1 9 within not imm-error
               swap sdreg r> + opsize+ h, endof
    invalid endcase endif clean ;
$E100 shift: asl,   $E101 shift: lsl,   $E102 shift: roxl,  $E103 shift: rol,
$E000 shift: asr,   $E001 shift: lsr,   $E002 shift: roxr,  $E003 shift: ror,

: bit: ( opcode "name" -- ) create host, does> ( arg1 arg2 -- )
    @ >r 2arg case
        dd' of <long> swap sdreg $100 + r> + h, endof
        dm' of <byte> drop  dreg $100 + r> + ea, endof
        #d' of <long>  nip  sreg $800 + r> + h, h, endof
        #m' of <byte> 2drop $800 r> + ea @ + opsize+ h, h, ext1, endof
    invalid endcase clean ;
$0000 bit: bittest, aka btst,       $0040 bit: bitchg, aka bchg,
$0080 bit: bitclr,  aka bclr,       $00C0 bit: bitset, aka bset,

: math: ( opcode "name" -- ) create host, does> ( arg1 arg2 -- )
    <word> @ >r 2arg case
        dd' of sdreg r> + h, endof
        md' of nip dreg r> + ea, endof
        #d' of nip dreg r> + w imm, endof
    invalid endcase clean ;
$80C0 math: divu,   $81C0 math: divs,   $C0C0 math: mulu,   $C1C0 math: muls,

: compare, ( arg1 arg2 -- )
    2arg case
        dd' of sdreg $B000 + opsize+ h, endof
        ad' of nobyte sdreg $B008 + opsize+ h, endof
        md' of nip dreg $B000 + ea, endof
        #d' of nip dreg $B000 + opsize+ imm, endof
        da' of nobyte sdreg $B0C0 + opsize @ 4 and 6 lshift + h, endof
        aa' of nobyte adreg $B0C0 + opsize @ 4 and 6 lshift + h, endof
        ma' of nobyte nip dreg $B0C0 + 
               ea @ + opsize @ 4 and 6 lshift + h, ext1, endof
        #a' of nobyte nip dreg $B0C0 + opsize @ 4 and 6 lshift + imm, endof
        #m' of 2drop $0C00 ea @ + opsize+ h, imm#, ext1, endof
        mm' of 2drop --? not effect-error (easd) $B108 opsize+ h, endof
    invalid endcase clean ;  aka cmp,

: (csize+) ( n -- n' ) opsize @ 2 and 5 lshift + ;
: check, ( mem dreg -- )
    <word> 2arg case
        dd' of sdreg $4100 + (csize+) h, endof
        md' of nip dreg $4100 + (csize+) ea @ + h, ext1, endof
        #d' of nip dreg $4100 + (csize+) imm, endof
    invalid endcase clean ;  aka chk,

: (size+)  ( n -- n' ) opsize @ dup 2/ or 3 and 12 lshift + ;
: (eadest) ( n ea -- n' ) @ dup dreg swap $38 and 3 lshift + + ;
: move, ( arg1 arg2 -- )
    2arg case
        dd' of sdreg (size+) h, endof
        ad' of adreg (size+) h, endof
        da' of sdreg (size+) $40 + h, endof
        aa' of adreg (size+) $40 + h, endof
        md' of  nip dreg (size+) ea @ + h, ext1, endof
        dm' of drop sreg (size+) ea  (eadest) h, ext1, endof
        mm' of 2drop   0 (size+) ea' (eadest) ea @ + h, ext1, ext2, endof
        ma' of  nip dreg (size+) $40 + ea @ + h, ext1, endof
        am' of drop areg (size+) ea  (eadest) h, ext1, endof
        #d' of nip dreg over ?schar not nip opsize @ 4 <> or
               if (size+) imm, else swap $FF and + $7000 + h, endif endof
        #a' of  nip dreg (size+) $40 + imm, endof
        #m' of 2drop   0 (size+) ea (eadest) imm, ext1, endof
        ds' of      <status> sreg $44C0 + h, endof
        ms' of  nip <status> ea @ $44C0 + h, ext1, endof
        #s' of  nip <status>      $46C0   imm, endof
        sd' of swap <status> sreg $40C0 + h, endof
        sm' of drop <status> ea @ $40C0 + h, ext1, endof
    invalid endcase clean ;

: (disp) ( -- ) ea @ $38 and %101000 <> effect-error ;
: (ugh)  ( n -- n' ) opsize @ 4 and 4 lshift + ;
: movep, ( arg1 arg2 -- )
    nobyte 2arg case
        dm' of drop (disp) dreg $0188 + (ugh) ea @ 7 and + h, ext1, endof
        md' of  nip (disp) dreg $0108 + (ugh) ea @ 7 and + h, ext1, endof
    invalid endcase clean ;

: (ctrl) ( n -- n' ) ea @ dup $38 and dup $18 = swap $20 = or effect-error + ;
:  pea, ( mem -- )
    1arg m' <> invalid-error drop $4840 (ctrl) h, ext1, clean ;
:  lea, ( mem areg -- )
    2arg ma' <> invalid-error nip dreg $41C0 + (ctrl) h, ext1, clean ;

: exg, ( reg reg -- )
    2arg case dd' of $F140 endof aa' of $F148 endof
              ad' of $F188 endof da' of swap $F188 endof invalid endcase
    swap sdreg + h, clean ;

: ext, ( reg ) nobyte 1arg d' <> invalid-error sreg $4880 + (ugh) h, clean ;

: clear, ( arg -- )
    1arg case
        d' of opsize @ 4 =
              if dreg $7000 else sreg $4200 opsize+ endif + h, endof
        a' of nobyte dreg $3040 + 0 swap h imm, endof
        m' of drop $4200 ea, endof
    invalid endcase clean ;  aka clr,

: link, ( n # areg -- ) 
    <long> 2arg #a' <> invalid-error sreg $4E50 + h, drop w imm#, clean ;
: unlink, ( areg -- )
    <long> 1arg a' <> invalid-error sreg $4E58 + h, clean ;  aka unlk,

: swap, ( dreg -- ) <long> 1arg d' <> invalid-error sreg $4840 + h, clean ;
: stop,  ( n # -- ) <word> } # { <> invalid-error $4E72 h, w imm#,  clean ;
: trap,  ( n # -- ) <long> } # { <> invalid-error 15 and $4E40 + h, clean ;
: illegal,   ( -- ) <long> $4AFC h, clean ;
: reset,     ( -- ) <long> $4E70 h, clean ;
: nop,       ( -- ) <long> $4E71 h, clean ;
: ireturn,   ( -- ) <long> $4E73 h, clean ;  aka rte,
: return,    ( -- ) <long> $4E75 h, clean ;  aka rts,
: trapv,     ( -- ) <long> $4E76 h, clean ;
: ccreturn,  ( -- ) <long> $4E77 h, clean ;  aka rtr,

: branch?, ( disp cc -- )
    $6000 +cc <word> swap ?shalf not disp-error
    ?schar if $FF and + else swap h, endif h, clean ;   aka bra?,
: branch, ( disp -- ) yes branch?, ;                    aka bra,
: brasub, ( disp -- )  no branch?, ;                    aka bsr,

: decbra?, ( reg disp cc -- )
    <word> rot 1arg d' <> invalid-error
    sreg +cc $50C8 + h, w imm#, clean ;                 aka dbra?,
: decbra, ( reg disp -- ) no decbra?, ;                 aka dbra,

: jump: ( opcode "name" -- ) create host, does> ( mem -- )
    @ swap 1arg m' <> invalid-error drop (ctrl) h, ext1, clean ;
$4EC0 jump: jump, aka jmp,      $4E80 jump: jumpsub, aka jsr,

( ---------------------------------------------------------------------------- )
(       Move 'em                                                               )
( ---------------------------------------------------------------------------- )
$A000 make [[
: ]] ( [[ .. -- mask )
    0 begin over [[ <> while
        swap 1arg case d' of 7 and endof a' of 15 and endof invalid endcase
        1 swap lshift or
    repeat nip ;
: >[]< ( u -- u )
    0 8 0 do over $0001 i lshift and 16 i 2* 1+ - lshift + 
             over $8000 i rshift and 16 i 2* 1+ - rshift + loop swap drop ;
: movem, ( arg1 arg2 -- )
    nobyte 1arg m' = if
        drop ea @ $38 and $20 = if >[]< $4880 ea @ + else $4880 (ctrl) endif
        (ugh) h, ext1, h, clean exit endif
    swap 1arg m' = if
        drop $4C80 ea @ $38 and $18 = if ea @ + else (ctrl) endif
        (ugh) h, ext1, h, clean exit endif
    invalid ;

( ---------------------------------------------------------------------------- )
(       Macro Instructions                                                     )
( ---------------------------------------------------------------------------- )
:  push, ( reg -- ) -[sp]       move, ;
:  poke, ( reg -- )  [sp]       move, ;
:  pull, ( reg -- )  [sp]+ swap move, ;
:  peek, ( reg -- )  [sp]  swap move, ;
: rpush, ( reg -- ) -[rp]       move, ;
: rpoke, ( reg -- )  [rp]       move, ;
: rpull, ( reg -- )  [rp]+ swap move, ;
: rpeek, ( reg -- )  [rp]  swap move, ;
:  read, ( reg -- )  [tp]+ swap move, ;
:   inc, ( reg -- )  1 } # { rot add, ;
:   dec, ( reg -- )  1 } # { rot sub, ;
: b-ext, ( reg -- )  dup w ext,  ext, ;     aka c-ext,

( ---------------------------------------------------------------------------- )
(       Flow Control                                                           )
( ---------------------------------------------------------------------------- )
: displacement  ( n -- n' ) romspace - 2 - ;
: displacement> ( n -- n' ) romspace swap - 2 - ;

: begin    ( -- dest ) romspace ;
: again    ( dest -- ) displacement branch, ;
: until ( dest cc -- ) $100 xor >r displacement r> branch?, ;

: if ( cc -- cc orig ) $100 xor romspace 0 h, ;
: then  ( cc orig -- )
    dup displacement> ?schar not disp-error
    $FF and rot $6000 +cc + swap romh! clean ;   aka endif

: ahead  ( -- cc orig )              no } if { ;
: else   ( cc orig -- cc orig )      } ahead { 2swap } then { ;
: while  ( dest cc -- cc orig dest ) } if { rot ;
: repeat ( cc orig dest -- )         } again then { ;

: do ( n reg -- reg dest )
    1arg d' <> invalid-error swap 1- } # { third h move, } begin { ;
: loop  ( reg dest -- ) displacement decbra, ;
: loop? ( reg dest cc -- ) $100 xor >r displacement r> decbra?, ;

( ---------------------------------------------------------------------------- )
(       Callers                                                                )
( ---------------------------------------------------------------------------- )
: (call,) ( addr jumpxt branchxt -- )
    third displacement ?shalf if swap execute 2drop exit endif
    2drop >r [#] r> execute ;

: primitive,  ( addr -- )    ['] jump,    ['] branch, (call,) ;
: subroutine, ( addr -- )    ['] jumpsub, ['] brasub, (call,) ;
: primitive?, ( addr cc -- ) >r displacement r> branch?, ;
: execute,    ( xt -- )      [#] dfa lea, [dfa]+ a1 move, [a1] jump, ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )














