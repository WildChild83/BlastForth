( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Helpful Helpers                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
only Forth definitions   decimal

wordlist constant blast-wordlist
get-order blast-wordlist swap 1+ set-order definitions

( ---------------------------------------------------------------------------- )
: c!+ ( c addr -- addr' ) tuck c! 1+ ;
:  !+ ( n addr -- addr' ) tuck  ! cell+ ;
:  @+ ( addr -- addr' n ) dup @ swap cell+ ;

: >order     ( wid -- ) >r get-order r> swap 1+ set-order ;
: third    ( a b c -- a b c a )   sp@ 2 cells + @ ;
: fourth ( a b c d -- a b c d a ) sp@ 3 cells + @ ;

: synonym ( "newname" "oldname" -- ) parse-name  nextname  ' alias ;
: aka     ( "name -- ) lastxt alias ;
synonym flag 0<>
synonym not  0=

( ---------------------------------------------------------------------------- )
:     bytes ( n -- n' ) ;
: kilobytes ( n -- n' ) 10 lshift ;
: megabytes ( n -- n' ) 20 lshift ;

: ?schar ( n -- n flag ) dup        -128        128 within ;
: ?shalf ( n -- n flag ) dup      -32768      32768 within ;
: ?scell ( n -- n flag ) dup -2147483648 2147483648 within ;
:  ?char ( n -- n flag ) dup        -128        256 within ;
:  ?half ( n -- n flag ) dup      -32768      65536 within ;
:  ?cell ( n -- n flag ) dup -2147483648 4294967296 within ;

: cleave ( n u -- n1 n2 ) 2dup invert and -rot and ;
: cellcount ( addr -- addr' n ) dup cell+ swap @ ;
: ndrop  ( xn..x1 n -- ) cells sp@ + cell+ sp! ;

: list,   ( xn..x1 n -- ) dup , 0 swap 1- ?do i roll , -1 +loop ;
: list@   ( addr -- xn..x1 n ) cellcount dup >r 0 ?do @+ loop drop r> ;
: string! ( addr u addr' -- ) 2dup c! 1+ swap cmove ;

: .n ( n n' -- )
    ." ." >r s>d <# #s #> r@ min r> over - 0 max 0 ?do ." 0" loop type space ;
: megabytes. ( n -- )
    1 megabytes /mod 0 .r   1000000 $100000 */ 2 .n   ." Megabytes " ;
: kilobytes. ( n -- )
    dup 1 megabytes > if megabytes. exit endif
    1 kilobytes /mod 0 .r   1000 $400  */ 2 .n   ." kilobytes " ;
: bytes. ( n -- ) dup 1 kilobytes > if kilobytes. exit endif   . ." bytes" ;

( ---------------------------------------------------------------------------- )
(       Glossaries                                                             )
( ---------------------------------------------------------------------------- )
1 kilobytes  constant glosscap
create   glossnames   glosscap allot         glossnames glosscap erase
variable glossptr     glossnames glossptr !
: glosscap?   glossptr @ glossnames - glosscap > abort" Too many glossaries." ;
: gloss+ ( addr u wid -- )
    glossptr @ !+   2dup + 1+ aligned glossptr !   glosscap?   string! ;
: glossname ( wid -- )
    glossnames begin cellcount ?dup while third <> while count + aligned repeat
    nip count type space exit   then 2drop ." - " ;
: (glossary) ( wid "name" -- )
    >r save-input parse-name r@ gloss+ restore-input throw
    r> >order   get-order create list,   does> ( -- ) list@ set-order ;
: Glossary ( "name" -- ) wordlist (glossary) ;

bl parse Forth  forth-wordlist gloss+

: } ( -- ) previous ; immediate

( ---------------------------------------------------------------------------- )
blast-wordlist (glossary) Blast definitions

: order ( -- ) get-order 0 ?do glossname loop 3 spaces get-current glossname ;
: {     ( -- ) also Forth ; immediate
: Forth ( -- ) only Forth ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )


















