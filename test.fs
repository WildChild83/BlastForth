

( ---------------------------------------------------------------------------- )
include system.fs

Forth definitions

  $BEEF constant signal
$C00000 constant vdp-data
$C00004 constant vdp-ctrl

hvalue hello

10 buffer: something
 6 buffer: something-else

code backcolor ( u -- )
    vdp-data [#] a1 lea, $C0000000 # [a1 4 +] move,
    tos [a1] h move, tos pop, next
code anotherword
    hello [#] a2 lea, $2B00B1E5 # [a2] move, next
: aword ( -- ) $00EE backcolor   1000 ms  $0E0E backcolor ;


68k-start: asm
    [rp -128 +] sp lea, tos clear,
    next& # np move,    
    $4BFA0004 , next ]

    aword signal
    anotherword  something-else
    hello
    begin again ;


( ---------------------------------------------------------------------------- )
{ cr romfile: test.md
\ { cr printrom

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )













