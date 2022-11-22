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
Forth definitions {

( ---------------------------------------------------------------------------- )
(       Binary File Output                                                     )
( ---------------------------------------------------------------------------- )
variable rom        SizeOfROM allocate throw  rom !
variable rom*       rom @ rom* !

: >1< ( n -- c ) $FF and ;
: >2< ( n -- u ) dup  8 rshift >1< swap >1<  8 lshift or ;
: >4< ( n -- u ) dup 16 rshift >2< swap >2< 16 lshift or ;

:  , ( n -- ) >4<  rom* @ tuck l!  4 + rom* ! ;    aka l,
: h, ( n -- ) >2<  rom* @ tuck w!  2 + rom* ! ;    aka w,
: c, ( n -- ) >1<  rom* @ tuck c!  1+  rom* ! ;    aka b,

: zeroes, ( n -- ) rom* @ 2dup + swap ?do 0 i c! loop rom* +! ;

:  string, ( addr u -- ) rom* @   2dup + rom* !   swap cmove ;
: cstring, ( addr u -- ) dup c, string, ;

: romaddr ( addr -- romaddr ) rom @ + ;
: roml@ ( romaddr -- u ) romaddr l@ >4< ;           aka rom@
: romw@ ( romaddr -- u ) romaddr w@ >2< ;           aka romh@
: romb@ ( romaddr -- u ) romaddr c@ >1< ;           aka romc@
: roml! ( u romaddr -- ) >r >4< r> romaddr l! ;     aka rom!
: romw! ( u romaddr -- ) >r >2< r> romaddr w! ;     aka romh!
: romb! ( u romaddr -- ) >r >1< r> romaddr c! ;     aka romc!

:  romsize ( -- u ) rom* @ rom @ - ;
: alignrom   ( -- ) rom* @ 1 and rom* +! ;
: romstats   ( -- ) romsize .bytes ;
:  freerom   ( -- ) romstats  rom  @ free throw   rom off  rom* off
                    cr depth if .s cr endif ;
: printrom   ( -- ) rom* @ rom @ 512 + ?do i c@ hex. loop cr freerom ;

: romfile  ( addr u -- )
    2dup type ." , "   w/o bin create-file throw >r
    rom @ rom* @ over - r@ write-file throw   r> close-file throw   freerom ;
: romfile: ( "name" -- ) parse-name romfile ;
: romfile" ( "name" -- ) [char] " parse romfile ;

( ---------------------------------------------------------------------------- )
(       68k Vector Table                                                       )
( ---------------------------------------------------------------------------- )
: !68k-vector ( n -- ) romsize swap  2 lshift rom! ;

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
}
256 zeroes,                     

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















