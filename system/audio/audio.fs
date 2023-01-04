( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Sound Hardware Drivers                                                 )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - romfile.fs                                                       )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions   

$A00000 constant z80ram
$A11100 constant z80request     aka z80status
$A11200 constant z80reset

( ---------------------------------------------------------------------------- )
code z80[[ ( -- )
    z80reset [#] a1 address, sr -[rp] h move, 2700 # sr h move,
    0 # [a1] h move, $100 # [a1 -256 +] h move, $100 # [a1] h move, next
code ]]z80 ( -- )
    z80reset [#] a1 address, 0 # [a1] h move, d1 20 for loop
    $100 # [a1] h move, 0 # [a1 -256 +] h move, [rp]+ sr h move, next
code z80[  ( -- )
    z80request [#] a1 address, sr -[rp] h move, 2700 # sr h move,
    $100 # [a1] h move, begin 0 # [a1] test-bit, z= until next
code ]z80  ( -- )
    0 # z80request [#] h move, [rp]+ sr h move, next

: move>z80    ( addr u -- ) z80ram  swap z80[[ cmove ]]z80 ;
: move>z80-at ( addr u z80addr -- ) swap z80[  cmove  ]z80 ;

: z80! ( c z80addr -- ) z80[ c! ]z80 ;
: z80@ ( z80addr -- c ) z80[ c@ ]z80 ;

{:} sound-driver: ( addr u "name" -- )
    create h, ,  does> ( -- ) dup half+ @ swap h@ move>z80 ;

( ---------------------------------------------------------------------------- )
Glossary Z80 definitions {

include z80.fs

romspace Z80 Assembler
    a xor, 8192 39 - # bc load, 39 # de load, 38 # hl load, hl sp load,
    a [hl] load, +copy, ix pull, iy pull, a i load, a r load,
    de pull, hl pull, af pull, af><af, exchange, bc pull,
    de pull, hl pull, af pull, hl sp load, -interrupts, imode1,
    $E9 # [hl] load, [hl] jump,
romspace over -

Forth definitions Z80
sound-driver: silence

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )















