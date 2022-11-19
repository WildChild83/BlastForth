( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Motorola 68000 CPU Assembler                                           )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies                                                           )
(           - romfile.fs                                                       )
(           - the following words must be defined:                             )
(               asm,  asm!  asmsize                                            )
(                                                                              )
(       TODO:                                                                  )
(           - PC-relative addressing                                           )
(           - more testing                                                     )
(                                                                              )
( ---------------------------------------------------------------------------- )

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
$1000 constant tos  $1004 constant d4   $2008 constant tp   $200C constant a4
$1001 constant d1   $1005 constant d5   $2009 constant a1   $200D constant np
$1002 constant d2   $1006 constant d6   $200A constant a2   $200E constant sp
$1003 constant d3   $1007 constant d7   $200B constant a3   $200F constant rp
$3001 constant ccr  $3002 constant sr   $3003 constant usp

( ---------------------------------------------------------------------------- )
(       Condition Codes                                                        )
( ---------------------------------------------------------------------------- )
$4000 constant yes                      $4800 constant vclr
$4100 constant no                       $4900 constant vset
$4200 constant ugt                      $4A00 constant nclr aka pos
$4300 constant ult=                     $4B00 constant nset aka neg
$4400 constant cclr aka ugt=            $4C00 constant gt=
$4500 constant cset aka ult             $4D00 constant lt
$4600 constant z<>                      $4E00 constant gt
$4700 constant z=                       $4F00 constant lt=
: +cc ( cc n -- n' ) swap $FFFFF0FF cleave $4000 <> cc-error  + ;

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
$5000 constant #

: long, ( n -- ) dup 16 rshift asm, asm, ;
: imm#, ( n -- )
    opsize @ case
        1 of ?char not imm-error $FF and asm, endof
        2 of ?half not imm-error         asm, endof
        4 of ?cell not imm-error        long, endof
    -1 opsize-error endcase ;

( ---------------------------------------------------------------------------- )
(       Effective Addresses                                                    )
( ---------------------------------------------------------------------------- )
variable ea         variable ea'
variable ext1       variable ext2       variable ext
: addr! ( n -- ) ea dup @ if drop ea' ext2 ext ! endif ! ;
: addr: ( n "name" -- ) create , does> ( -- arg ) @ addr! $8000 ;

%010000 addr: [tp]  %011000 addr: [tp]+  %100000 addr: -[tp]  %101000 addr: [tp
%010001 addr: [a1]  %011001 addr: [a1]+  %100001 addr: -[a1]  %101001 addr: [a1
%010010 addr: [a2]  %011010 addr: [a2]+  %100010 addr: -[a2]  %101010 addr: [a2
%010011 addr: [a3]  %011011 addr: [a3]+  %100011 addr: -[a3]  %101011 addr: [a3
%010100 addr: [a4]  %011100 addr: [a4]+  %100100 addr: -[a4]  %101100 addr: [a4
%010101 addr: [np]  %011101 addr: [np]+  %100101 addr: -[np]  %101101 addr: [np
%010110 addr: [sp]  %011110 addr: [sp]+  %100110 addr: -[sp]  %101110 addr: [sp
%010111 addr: [rp]  %011111 addr: [rp]+  %100111 addr: -[rp]  %101111 addr: [rp

: (ndx) ( addr -- flag ) dup @ $38 and $28 <> dup if nip exit endif $8 rot +! ;    
:  ndx: ( n "name" -- ) create , does> ( -- )
    ea (ndx) if ea' (ndx) effect-error endif @ ext @ +! ;

$080F ndx: tos+     $480F ndx: d4+      $880F ndx: tp+      $C80F ndx: a4+
$180F ndx: d1+      $580F ndx: d5+      $980F ndx: a1+      $D80F ndx: np+
$280F ndx: d2+      $680F ndx: d6+      $A80F ndx: a2+      $E80F ndx: sp+
$380F ndx: d3+      $780F ndx: d7+      $B80F ndx: a3+      $F80F ndx: rp+

: iw   ( -- ) ext @ dup @ $F7FF and swap ! ;        synonym ih iw
: #] ( n -- ) ?shalf not 1 and %111000 + addr!  ext @ !  $8000 ;
: +] ( n -- )
    ext @ dup >r @ ?dup-if
        $FF00 and swap ?schar not disp-error $FF and + r> !
    else ?shalf not disp-error $FFFF and r> ! endif ;
: 0] ( -- ) 0 +] ;

: <ext> ( ext ea n -- ext ea ) third @ swap 1 = if long, exit endif asm, ;
: (ext) ( ext ea -- )
    dup @ $38 and case
        $28 of over @ asm, endof
        $30 of over @ asm, endof
        $38 of dup @ 7 and dup 4 < if <ext> else drop endif endof               
    endcase 2drop ;
: ext1, ( -- ) ext1 ea  (ext) ;
: ext2, ( -- ) ext2 ea' (ext) ;

( ---------------------------------------------------------------------------- )
(       Operand Processors                                                     )
( ---------------------------------------------------------------------------- )
$9001 constant  d'  $9002 constant  a'  $9008 constant  m'  $9031 constant sd'
$9011 constant dd'  $9012 constant da'  $9018 constant dm'  $9013 constant ds'
$9021 constant ad'  $9022 constant aa'  $9028 constant am'  $9083 constant ms'
$9051 constant #d'  $9052 constant #a'  $9058 constant #m'  $9053 constant #s'
$9081 constant md'  $9082 constant ma'  $9088 constant mm'  $9038 constant sm'

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
:  ea,  ( n -- ) ea @ + opsize+ asm, ext1, ;
: imm,  ( n opcode -- ) %111100 + asm, imm#, ;
: clean ( -- ) ext1 off ext2 off ext1 ext !  ea off ea' off  4 opsize ! ; clean

( ---------------------------------------------------------------------------- )
: (easd)   ( -- n ) ea @ ea' @ sdreg ;
: (string) ( arg1 arg2 addr -- )
    @ >r 2arg case
        dd' of sdreg r> + asm, endof
        mm' of 2drop --? not effect-error  (easd)  r> + 8 + asm, endof
    invalid endcase clean ;
:  bcd: ( opcode "name" -- ) create , does> ( arg1 arg2 -- ) <byte> (string) ;
: addx: ( opcode "name" -- ) create , does> ( arg1 arg2 -- ) (string) ;
$C100 bcd: abcd,    $8100 bcd: sbcd,    $D100 addx: addx,   $9100 addx: subx,

: (single) ( n opcode -- ) >r 1arg case
        d' of sreg r> + opsize+ asm, endof
        m' of drop r> ea, endof
    invalid endcase clean ;
: set?, ( arg cc -- ) <byte> $50C0 +cc (single) ;
: nbcd,    ( arg -- ) <byte> $4800     (single) ;
: test,    ( arg -- )
    1arg a' = if 15 and $4A00 + opsize+ asm, clean exit endif $4A00 (single) ;
: single: ( opcode "name" -- ) create , does> ( arg -- ) @ (single) ;
$4000 single: negx,     $4400 single: neg,      $4600 single: not,

: (and1) ( n opcode -- n' ) $F080 and + ;
: (and2) ( n opcode -- n' ) $0F00 and + ;
: (data) ( arg1 arg2 opcode -- )
    >r 2arg case
        dd' of sdreg r> (and1) opsize+ asm, endof
        dm' of drop dreg $100 + r> (and1) ea, endof
        #d' of  nip sreg r> (and2) opsize+ asm, imm#, endof
        #m' of 2drop r> $0F00 and ea @ + opsize+ asm, imm#, ext1, endof
    invalid endcase clean ;
: <status>  case ccr of noword b endof sr of nobyte w endof invalid endcase ;
: (status) ( arg1 arg2 opcode -- )
    >r 2arg #s' <> if r> (data) exit endif
    nip <status> 0 r> (and2) opsize+ imm, clean ;
: xor, ( arg1 arg2 -- ) $B000 (status) ;            aka eor,

: (logic) ( arg1 arg2 opcode -- )
    >r 2arg md' = if nip dreg r> (and1) ea, clean exit endif r> (status) ;
: logic: ( opcode "name" -- ) create , does> ( arg1 arg2 -- ) @ (logic) ;
$C200 logic: and,       $8000 logic: or,

: (quick) ( n arg opcode -- )
    >r 1arg case
        d' of swap sdreg r> (and1) opsize+ asm, endof
        a' of swap adreg r> (and1) opsize+ asm, endof
        m' of drop  dreg r> (and1) ea, endof
    invalid endcase clean ;
: arithmetic: ( opcodes "name" -- ) create swap , , does> ( arg1 arg2 -- )
    third # = if fourth 1 9 within if rot drop cell+ @ (quick) exit endif endif
    @ >r 2arg case
        da' of nobyte sdreg r> (and1) opmode+ asm, endof
        ad' of nobyte adreg r> (and1) opsize+ asm, endof
        aa' of nobyte adreg r> (and1) opmode+ $4 + asm, endof
        #a' of nobyte  dreg r> (and1) opmode+ imm, endof
        md' of    nip  dreg r> (and1) ea, endof
        drop r> (data) exit
    endcase clean ;
$D600 $5000 arithmetic: add,        $9401 $5100 arithmetic: sub,

: shift: ( opcode "name" -- ) create , does> ( arg1 arg2 -- )
    @ 3 cleave third $8000 = if <byte> 9 lshift + nip $C0 + ea, else
    3 lshift + >r 2arg case
        dd' of swap sdreg $20 + r> + opsize+ asm, endof
        #d' of nip over 1 9 within not imm-error
               swap sdreg r> + opsize+ asm, endof
    invalid endcase endif clean ;
$E100 shift: asl,   $E101 shift: lsl,   $E102 shift: roxl,  $E103 shift: rol,
$E000 shift: asr,   $E001 shift: lsr,   $E002 shift: roxr,  $E003 shift: ror,

: bit: ( opcode "name" -- ) create , does> ( arg1 arg2 -- )
    @ >r 2arg case
        dd' of <long> swap sdreg $100 + r> + asm, endof
        dm' of <byte> drop  dreg $100 + r> + ea, endof
        #d' of <long>  nip  sreg $800 + r> + asm, asm, endof
        #m' of <byte> 2drop      $800   r> + ea @ + opsize+ asm, asm, ext1, endof
    invalid endcase clean ;
$0000 bit: bittest, aka btst,       $0040 bit: bitchg, aka bchg,
$0080 bit: bitclr,  aka bclr,       $00C0 bit: bitset, aka bset,

: math: ( opcode "name" -- ) create , does> ( arg1 arg2 -- )
    <word> @ >r 2arg case
        dd' of sdreg r> + asm, endof
        md' of nip dreg r> + ea, endof
        #d' of nip dreg r> + w imm, endof
    invalid endcase clean ;
$80C0 math: divu,   $81C0 math: divs,   $C0C0 math: mulu,   $C1C0 math: muls,

: comp, ( arg1 arg2 -- )
    2arg case
        dd' of sdreg $B000 + opsize+ asm, endof
        ad' of nobyte sdreg $B008 + opsize+ asm, endof
        md' of nip dreg $B000 + ea, endof
        #d' of nip dreg $B000 + opsize+ imm, endof
        da' of nobyte sdreg $B0C0 + opsize @ 4 and 6 lshift + asm, endof
        aa' of nobyte adreg $B0C0 + opsize @ 4 and 6 lshift + asm, endof
        ma' of nobyte nip dreg $B0C0 + 
               ea @ + opsize @ 4 and 6 lshift + asm, ext1, endof
        #a' of nobyte nip dreg $B0C0 + opsize @ 4 and 6 lshift + imm, endof
        #m' of 2drop $0C00 ea @ + opsize+ asm, imm#, ext1, endof
        mm' of 2drop --? not effect-error (easd) $B108 opsize+ asm, endof
    invalid endcase clean ;  aka cmp,

: (csize+) ( n -- n' ) opsize @ 2 and 5 lshift + ;
: check, ( mem dreg -- )
    <word> 2arg case
        dd' of sdreg $4100 + (csize+) asm, endof
        md' of nip dreg $4100 + (csize+) ea @ + asm, ext1, endof
        #d' of nip dreg $4100 + (csize+) imm, endof
    invalid endcase clean ;  aka chk,

: (size+)  ( n -- n' ) opsize @ dup 2/ or 3 and 12 lshift + ;
: (eadest) ( n ea -- n' ) @ dup dreg swap $38 and 3 lshift + + ;
: move, ( arg1 arg2 -- )
    2arg case
        dd' of sdreg (size+) asm, endof
        ad' of adreg (size+) asm, endof
        da' of sdreg (size+) $40 + asm, endof
        aa' of adreg (size+) $44 + asm, endof
        md' of  nip dreg (size+) ea @ + asm, ext1, endof
        dm' of drop sreg (size+) ea  (eadest) asm, ext1, endof
        mm' of 2drop   0 (size+) ea' (eadest) ea @ + asm, ext1, ext2, endof
        ma' of  nip dreg (size+) $40 + ea @ + asm, ext1, endof
        am' of drop areg (size+) ea @ + asm, ext1, endof
        #d' of nip dreg over ?schar not nip opsize @ 4 <> or
               if (size+) imm, else swap $FF and + $7000 + asm, endif endof
        #a' of  nip dreg (size+) $40 + imm, endof
        #m' of 2drop   0 (size+) ea (eadest) imm, ext1, endof
        ds' of      <status> sreg $44C0 + asm, endof
        ms' of  nip <status> ea @ $44C0 + asm, ext1, endof
        #s' of  nip <status>      $44C0   imm, endof
        sd' of swap <status> sreg $40C0 + asm, endof
        sm' of drop <status> ea @ $40C0 + asm, ext1, endof
    invalid endcase clean ;

: (disp) ( -- ) ea @ $38 and %101000 <> effect-error ;
: (ugh)  ( n -- ) opsize @ 4 and 4 lshift + ;
: movep, ( arg1 arg2 -- )
    nobyte 2arg case
        dm' of drop (disp) dreg $0188 + (ugh) ea @ 7 and + asm, ext1, endof
        md' of  nip (disp) dreg $0108 + (ugh) ea @ 7 and + asm, ext1, endof
    invalid endcase clean ;

: (ctrl) ( n -- n' ) ea @ dup $38 and dup $18 = swap $20 = or effect-error + ;
:  pea, ( mem -- ) 1arg m' <> invalid-error drop $4840 (ctrl) asm, ext1, clean ;
:  lea, ( mem areg -- )
    2arg ma' <> invalid-error nip dreg $41C0 + (ctrl) asm, ext1, clean ;

: exg, ( reg reg -- )
    2arg case dd' of $F140 endof aa' of $F148 endof
              ad' of $F188 endof da' of swap $F188 endof invalid endcase
    swap sdreg + asm, clean ;

: ext, ( reg ) nobyte 1arg d' <> invalid-error sreg $4880 + (ugh) asm, clean ;

: clear, ( mem -- )
    1arg case
        d' of sreg opsize @ 4 = if $7000 else $4200 opsize+ endif + asm, endof
        m' of drop $4200 ea, endof
    invalid endcase clean ;  aka clr,

: link, ( n # areg -- ) 
    <long> 2arg #a' <> invalid-error sreg $4E50 + asm, drop w imm#, clean ;
: unlink, ( areg -- )
    <long> 1arg a' <> invalid-error sreg $4E58 + asm, clean ;  aka unlk,

: swap, ( dreg -- ) <long> 1arg d' <> invalid-error sreg $4840 + asm, ;
: stop,  ( n # -- ) <word> # <> invalid-error $4E72 asm, w imm#, ;
: trap,  ( n # -- ) <long> # <> invalid-error 15 and $4E40 + asm, ;
: illegal,   ( -- ) <long> $4AFC asm, ;
: reset,     ( -- ) <long> $4E70 asm, ;
: nop,       ( -- ) <long> $4E71 asm, ;
: ireturn,   ( -- ) <long> $4E73 asm, ;  aka rte,
: return,    ( -- ) <long> $4E75 asm, ;  aka rts,
: trapv,     ( -- ) <long> $4E76 asm, ;
: ccreturn,  ( -- ) <long> $4E77 asm, ;  aka rtr,

: branch?, ( disp cc -- )
    $6000 +cc <word> swap ?shalf not disp-error
    ?schar if $FF and + else swap asm, endif asm, clean ;
: branch, ( disp -- ) yes branch?, ;
: brasub, ( disp -- )  no branch?, ;

: decbra?, ( reg disp cc -- )
    <word> rot 1arg d' <> invalid-error
    sreg +cc $50C8 + asm, w imm#, clean ;
: decbra, ( reg disp -- ) no decbra?, ;

: jump: ( opcode "name" -- ) create , does> ( mem -- )
    @ swap 1arg m' <> invalid-error drop (ctrl) asm, ext1, clean ;
$4EC0 jump: jump, aka jmp,      $4E80 jump: jumpsub, aka jsr,

( ---------------------------------------------------------------------------- )
(       Move 'em                                                               )
( ---------------------------------------------------------------------------- )
$A000 constant [[
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
        (ugh) asm, ext1, asm, clean exit endif
    swap 1arg m' = if
        drop $4C80 ea @ $38 and $18 = if ea @ + else (ctrl) endif
        (ugh) asm, ext1, asm, clean exit endif
    invalid ;

( ---------------------------------------------------------------------------- )
(       Macro Instructions                                                     )
( ---------------------------------------------------------------------------- )
:  push, ( reg -- ) -[sp]       move, ;
:  poke, ( reg -- )  [sp]       move, ;
:  pop,  ( reg -- )  [sp]+ swap move, ;
:  peek, ( reg -- )  [sp]  swap move, ;
: rpush, ( reg -- ) -[rp]       move, ;
: rpoke, ( reg -- )  [rp]       move, ;
: rpop,  ( reg -- )  [rp]+ swap move, ;
: rpeek, ( reg -- )  [rp]  swap move, ;
:  inc,  ( reg -- )  1 # rot add, ;
:  dec,  ( reg -- )  1 # rot sub, ;

( ---------------------------------------------------------------------------- )
(       Flow Control                                                           )
( ---------------------------------------------------------------------------- )
: displacement ( n -- n' ) asmsize - 2 - ;

: begin    ( -- dest ) asmsize ;
: again    ( dest -- ) displacement branch, ;
: until ( dest cc -- ) 1 xor >r displacement r> branch?, ;

: if ( cc -- cc orig ) 1 xor asmsize 0 asm, ;
: then  ( cc orig -- )
    dup displacement ?schar not disp-error
    $FF and rot $6000 +cc + swap asm! clean ;   aka endif

: ahead  ( -- cc orig )              no if ;
: else   ( cc orig -- cc orig )      ahead 2swap then ;
: while  ( dest cc -- cc orig dest ) if rot ;
: repeat ( cc orig dest -- )         again then ;

: do ( n reg -- reg dest )
    1arg d' <> invalid-error swap 1- # third move, begin ;
: loop  ( reg dest -- ) displacement decbra, ;
: loop? ( reg dest cc -- ) 1 xor >r displacement r> decbra?, ;

: primitive,  ( addr -- ) #] jump, ;
: subroutine, ( addr -- ) #] jumpsub, ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )













