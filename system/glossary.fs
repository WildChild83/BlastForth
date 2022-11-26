( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Environment Utilities                                                  )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
only Forth definitions   decimal

( ---------------------------------------------------------------------------- )
:  @+ ( addr -- n addr' ) dup @   swap cell+ ;
: @@+ ( addr -- n addr' ) dup @ @ swap cell+ ;
: cellcount ( addr -- addr' n ) dup cell+ swap @ ;
: list,  ( xn..x1 n -- ) dup , 1- 0 swap do i roll , -1 +loop ;
: list@  ( addr -- n*x  n ) cellcount dup >r 0 ?do  @+ loop drop r> ;
: list@@ ( addr -- n*x  n ) cellcount dup >r 0 ?do @@+ loop drop r> ;

[undefined] string, [if] : string, dup c, here swap dup allot cmove ; [then]

( ---------------------------------------------------------------------------- )
(       Copy words from one Glossary to another                                )
( ---------------------------------------------------------------------------- )
variable scribe
: -transcription    scribe off ;
: (refill) ( -- ) refill 0= abort" Unexpected end of input." ;
: execute? ( xt -- xt|0  )
    dup ['] \ =   over ['] ( = or   over ['] -transcription = or   and ;
: transcribe-name ( addr u -- flag )
    find-name dup 0= abort" Word not found."
    dup name>int dup execute? ?dup if execute 2drop exit endif
    swap name>string nextname alias ;
: transcribe ( "name" -- )
    parse-name ?dup if transcribe-name exit endif drop (refill) ;

( ---------------------------------------------------------------------------- )
(       Word list and search order for cross-compilation                       )
( ---------------------------------------------------------------------------- )
variable searchorder     variable compilation
: glosslist  ( -- addr )      searchorder @ cell+ count + aligned ;
: searchlist ( -- n*gloss n ) glosslist list@ ;
: widlist    ( -- n*wid n )   glosslist list@@ ;
: !order     ( gloss -- )     searchorder !   widlist set-order ;
: no-int     ( -- )           state @ 0= -14 and throw ;

wordlist create blast-gloss , bl parse Forth string, align  blast-gloss 1 list,
blast-gloss searchorder ! get-order blast-gloss @ swap 1+ set-order definitions

:  Forth         blast-gloss !order ;
:  words         words ;
:  definitions   searchorder @ compilation !  definitions ;
:  wid           searchorder @ @ ;
: >order         >r get-order r> swap 1+ set-order ;
:  transcription scribe on   begin transcribe   scribe @ 0= until ;
: -transcription scribe off ;
:  order         searchlist 0 ?do cell+ count type space loop compilation @
                 cell+ count [char] > emit type [char] < emit space ;
: Glossary ( "name" -- )
    wordlist >r   save-input create restore-input throw
    searchlist here r> , tuck >r 1+
    parse-name string, align   list,   r> !order
    does> ( -- ) !order ;

( ---------------------------------------------------------------------------- )
forth-wordlist
    Glossary Host definitions
>order

    include transcribe.fs

: } ( -- ) previous ; immediate
: host,  ( n -- )  , ;
: hostc, ( c -- ) c, ;
: name>interpret ( nt -- xt ) name>int ;

( ---------------------------------------------------------------------------- )
forth-wordlist
    Host wid  constant host-wordlist
    Forth definitions
>order

: \   postpone \ ; immediate
: (   postpone ( ; immediate
: synonym ( "newname" "oldname" ) parse-name  nextname  ' alias ;
: aka     ( "newname" )           lastxt alias ;

( ---------------------------------------------------------------------------- )
Host    : { ( -- ) host-wordlist >order ;   immediate

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Default Forth wordlist is gone, only "our" Forth exists now            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Host definitions
synonym flag 0<>     synonym not 0=

( ---------------------------------------------------------------------------- )
: third   2>r dup  2r> rot ;
: fourth  2>r over 2r> rot ;
:  rdrop  2r> nip >r       ;
: 2rdrop  r> 2r> 2drop >r  ;
: -rot    rot rot          ;
: -2rot   2rot 2rot        ;    aka 2-rot
:  umin   2dup u> if swap endif drop ;
:  umax   2dup u< if swap endif drop ;

( ---------------------------------------------------------------------------- )
: ?schar ( n -- n flag ) dup        -128        128 within ;
: ?shalf ( n -- n flag ) dup      -32768      32768 within ;
: ?scell ( n -- n flag ) dup -2147483648 2147483648 within ;
:  ?char ( n -- n flag ) dup        -128        256 within ;
:  ?half ( n -- n flag ) dup      -32768      65536 within ;
:  ?cell ( n -- n flag ) dup -2147483648 4294967296 within ;

( ---------------------------------------------------------------------------- )
: c!+ ( c addr -- addr' ) tuck c! 1+ ;
:  !+ ( n addr -- addr' ) tuck  ! cell+ ;
:  @+ ( addr -- addr' n ) dup @ swap cell+ ;
: cellcount ( addr -- addr' n ) dup cell+ swap @ ;
: demux  ( n u -- n1 n2 ) 2dup invert and -rot and ;
: bounds ( addr u -- limit index ) over + swap ;
: ndrop  ( n*x  n -- ) 0 ?do drop loop ;
: uppercase  ( c -- c' ) dup [ char a char z 1+ ] 2literal within 32 and - ;
: c=   ( c1 c2 -- flag ) uppercase swap uppercase =  ;
: c<>  ( c1 c2 -- flag ) uppercase swap uppercase <> ;
: str= ( addr1 u1 addr2 u2 -- flag )
    rot over <> if 3 ndrop false exit endif
    bounds ?do count i c@ c<> if drop unloop false exit endif loop drop true ;
: mem= ( addr1 u1 addr2 u2 -- flag ) compare 0= ;

( ---------------------------------------------------------------------------- )
:     bytes ( n -- n' ) ;
: kilobytes ( n -- n' ) 10 lshift ;
: megabytes ( n -- n' ) 20 lshift ;

: .n ( n n' -- )
    ." ." >r s>d <# #s #> r@ min r> over - 0 max 0 ?do ." 0" loop type space ;
: .megabytes ( n -- )
    1 megabytes /mod 0 .r   100000 $100000 */ 2 .n   ." Megabytes " ;
: .kilobytes ( n -- )
    dup 1 megabytes > if .megabytes exit endif
    1 kilobytes /mod 0 .r   100 1024  */ 2 .n   ." kilobytes " ;
: .bytes ( n -- ) dup 1 kilobytes > if .kilobytes exit endif   . ." bytes" ;

: hex. ( n -- ) base @ swap  [char] $ emit  hex .  base ! ;
: hex.s  ( -- ) base @ >r  hex .s   r> base ! ;

( ---------------------------------------------------------------------------- )
Forth definitions {

synonym include include
synonym [if]    [if]    immediate
synonym [else]  [else]  immediate
synonym [then]  [then]  immediate
synonym [endif] [then]  immediate
synonym bye     bye

: .( ( "text" -- ) [char] ) parse type ; immediate

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )


















