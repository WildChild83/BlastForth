( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Sega Genesis ROM File Creator                                          )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossaries.fs                                                    )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions {

1 megabytes constant SizeOfROM

( ---------------------------------------------------------------------------- )
(       Binary File Output                                                     )
( ---------------------------------------------------------------------------- )
variable rom        SizeOfROM allocate throw  rom !
variable rom*       rom @ rom* !

: >1< ( n -- c ) $FF and ;
: >2< ( n -- u ) dup  8 rshift >1< swap >1<  8 lshift or ;
: >4< ( n -- u ) dup 16 rshift >2< swap >2< 16 lshift or ;

: roml,   ( n -- ) >4<  rom* @ tuck l!  4 + rom* ! ;    synonym rom,  roml,
: romw,   ( n -- ) >2<  rom* @ tuck w!  2 + rom* ! ;    synonym romh, romw,
: romb,   ( n -- ) >1<  rom* @ tuck c!  1+  rom* ! ;    synonym romc, romb,
: zeroes, ( n -- ) rom* @ 2dup + swap ?do 0 i c! loop rom* +! ;

: rom$,  ( addr u -- ) rom* @   2dup + rom* !   swap cmove ;
: romc$, ( addr u -- ) dup romc, rom$, ;

: romaddr ( addr -- romaddr ) rom @ + ;
: roml@ ( romaddr -- u ) romaddr l@ >4< ;           synonym rom@  roml@
: romw@ ( romaddr -- u ) romaddr w@ >2< ;           synonym romh@ romw@
: romb@ ( romaddr -- u ) romaddr c@ >1< ;           synonym romc@ romb@
: roml! ( u romaddr -- ) >r >4< r> romaddr l! ;     synonym rom!  roml!
: romw! ( u romaddr -- ) >r >2< r> romaddr w! ;     synonym romh! romw!
: romb! ( u romaddr -- ) >r >1< r> romaddr c! ;     synonym romc! romb!

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

( ---------------------------------------------------------------------------- )
(       Sega ROM Header                                                        )
( ---------------------------------------------------------------------------- )
256 zeroes,                     s" SEGA GENESIS    " rom$,  \ 100 Console
                                s" (C)WILD 2019    " rom$,  \ 110 Copyrght
s" Japanese Name                                   " rom$,  \ 120 Domestic name
s" Western Name                                    " rom$,  \ 150 Int'l name
                                  s" GM 00000001-01" rom$,  \ 180 Version #
                                                   0 romh,  \ 18E Checksum
                                s" J               " rom$,  \ 190 I/O support
                                             $000000 rom,   \ 1A0 Start of ROM
                                        SizeOfROM 1- rom,   \ 1A4 End of ROM
                                             $FF0000 rom,   \ 1A8 Start of RAM
                                             $FFFFFF rom,   \ 1AC End of RAM
                                                   0 rom,   \ 1B0 SRAM enabled
                                                   0 rom,   \ 1B4 Unused
                                                   0 rom,   \ 1B8 Start of SRAM
                                                   0 rom,   \ 1BC End of SRAM
                                                   0 rom,   \ 1C0 Unused
                                                   0 rom,   \ 1C4 Unused
        s"                                         " rom$,  \ 1C8 Notes
                                s" JUE             " rom$,  \ 1F0 Country codes

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















