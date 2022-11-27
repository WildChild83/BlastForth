( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Sega Genesis ROM File Creator                                          )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(                                                                              )
( ---------------------------------------------------------------------------- )
Host definitions

variable rom        SizeOfROM allocate throw  rom !
variable rom*       rom @ rom* !

Verbose [IF] SizeOfROM .bytes .( allocated for ROM file.) [THEN]

: >1< ( n -- c ) $FF and ;
: >2< ( n -- u ) dup  8 rshift >1< swap >1<  8 lshift or ;
: >4< ( n -- u ) dup 16 rshift >2< swap >2< 16 lshift or ;

( ---------------------------------------------------------------------------- )
Forth definitions {

:  , ( n -- ) >4<  rom* @ tuck l!  4 + rom* ! ;    aka l,
: h, ( n -- ) >2<  rom* @ tuck w!  2 + rom* ! ;    aka w,
: c, ( n -- ) >1<  rom* @ tuck c!  1+  rom* ! ;    aka b,

: zeroes, ( n -- ) rom* @ 2dup + swap ?do 0 i c! loop rom* +! ;

:  ascii, ( addr u -- ) rom* @   2dup + rom* !   swap cmove ;
: #ascii, ( addr u -- ) dup c, ascii, ;
:  ascii" ( "text" -- ) [char] " parse  ascii, ;
: #ascii" ( "text" -- ) [char] " parse #ascii, ;

: romaddr ( addr -- romaddr ) rom @ + ;
: roml@ ( romaddr -- u ) romaddr l@ >4< ;           aka rom@
: romw@ ( romaddr -- u ) romaddr w@ >2< ;           aka romh@
: romb@ ( romaddr -- u ) romaddr c@ >1< ;           aka romc@
: roml! ( u romaddr -- ) >r >4< r> romaddr l! ;     aka rom!
: romw! ( u romaddr -- ) >r >2< r> romaddr w! ;     aka romh!
: romb! ( u romaddr -- ) >r >1< r> romaddr c! ;     aka romc!

: romspace ( -- u ) rom* @ rom @ - ;
: alignrom   ( -- ) rom* @ 1 and rom* +! ;
: romstats   ( -- ) romspace .bytes ;

Host definitions
: freerom ( -- )
    rom @ free throw   rom off rom* off
    cr depth if ." Stack Leak!!  " .s cr endif   bye ;

Forth definitions {
: printrom ( -- ) rom* @ rom @ 512 + ?do i c@ hex. loop cr freerom ;
: romfile  ( addr u -- )
    cr 2dup Verbose IF ." Program size: " ELSE type ." , " THEN romstats
    w/o bin create-file throw >r
        rom @ rom* @ over - r@ write-file throw
        r> close-file throw
    Verbose IF cr ." ROM file: " type ." , " SizeOfROM .bytes THEN
    freerom ;
: romfile: ( "name" -- ) parse-name romfile ;
: romfile" ( "name" -- ) [char] " parse romfile ;

( ---------------------------------------------------------------------------- )
(       68k Vector Table                                                       )
( ---------------------------------------------------------------------------- )
: !68k-vector ( n -- ) romspace swap  2 lshift rom! ;

: 68k-vbi:       ( -- ) 30 !68k-vector ;
: 68k-hbi:       ( -- ) 28 !68k-vector ;
: 68k-bus:       ( -- )  2 !68k-vector ;
: 68k-address:   ( -- )  3 !68k-vector ;
: 68k-illegal:   ( -- )  4 !68k-vector ;
: 68k-divzero:   ( -- )  5 !68k-vector ;
: 68k-check:     ( -- )  6 !68k-vector ;
: 68k-trapv:     ( -- )  7 !68k-vector ;
: 68k-privilege: ( -- )  8 !68k-vector ;
: 68k-trace:     ( -- )  9 !68k-vector ;
: 68k-uninit:    ( -- ) 15 !68k-vector ;
: 68k-spurious:  ( -- ) 24 !68k-vector ;

: 68k-trap: ( trap# -- ) 32 + !68k-vector ;
: 68k-start:      ( -- )  1   !68k-vector ;
: 68k-stack!    ( u -- )  0 rom! ;

256 zeroes,                     

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















