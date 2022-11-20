( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Environment Utilities                                                  )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
only Forth definitions   decimal

: } ( -- ) previous ; immediate

wordlist constant blast-wordlist
get-order blast-wordlist swap 1+ set-order definitions

( ---------------------------------------------------------------------------- )
:     bytes ( n -- n' ) ;
: kilobytes ( n -- n' ) 10 lshift ;
: megabytes ( n -- n' ) 20 lshift ;

: c!+ ( c addr -- addr' ) tuck c! 1+ ;
:  !+ ( n addr -- addr' ) tuck  ! cell+ ;
:  @+ ( addr -- addr' n ) dup @ swap cell+ ;

: >order     ( wid -- ) >r get-order r> swap 1+ set-order ;
: third    ( a b c -- a b c a )   2>r dup  2r> rot ;
: fourth ( a b c d -- a b c d a ) 2>r over 2r> rot ;
: ndrop ( xn..x1 n -- ) 0 ?do drop loop ;

: cellcount ( addr -- addr' n ) dup cell+ swap @ ;
: list,   ( xn..x1 n -- ) dup , 0 swap 1- ?do i roll , -1 +loop ;
: list@   ( addr -- xn..x1 n ) cellcount dup >r 0 ?do @+ loop drop r> ;
: string! ( addr u addr' -- ) 2dup c! 1+ swap cmove ;
: >type<  ( addr u -- ) [char] > emit   type   [char] < emit ;

( ---------------------------------------------------------------------------- )
(       Glossaries                                                             )
( ---------------------------------------------------------------------------- )
1 kilobytes  constant glosscap
create   glossnames   glosscap allot         glossnames glosscap erase
variable glossptr     glossnames glossptr !
: glosscap?   glossptr @ glossnames - glosscap > abort" Too many glossaries." ;
: gloss+ ( addr u wid -- )
    glossptr @ !+   2dup + 1+ aligned glossptr !   glosscap?   string! ;
: glossname ( wid -- addr u )
    glossnames begin cellcount ?dup while third <> while count + aligned repeat
    nip count exit   then 2drop s" -" ;
: (glossary) ( "name" -- ) get-order create list, does> ( -- ) list@ set-order ;
:  Glossary  ( "name" -- ) 
    save-input parse-name wordlist dup >order gloss+ restore-input throw
    (glossary) ;

( ---------------------------------------------------------------------------- )
bl parse Forth  forth-wordlist gloss+
bl parse Blast  blast-wordlist gloss+
also Forth definitions previous   (glossary) Blast definitions

:    {      ( -- ) also Forth ; immediate
:   Forth   ( -- ) only Forth ;
: >current< ( -- ) get-current glossname >type< space ;
:   order   ( -- ) get-order 0 ?do glossname type space loop >current< ;

: order> ( -- wid ) get-order over >r ndrop r> ;

( ---------------------------------------------------------------------------- )
(       Basic Stuff                                                            )
( ---------------------------------------------------------------------------- )
: synonym ( "newname" "oldname" -- ) parse-name  nextname  ' alias ;
: aka     ( "name -- ) lastxt alias ;
synonym flag 0<>
synonym not  0=

: ?schar ( n -- n flag ) dup        -128        128 within ;
: ?shalf ( n -- n flag ) dup      -32768      32768 within ;
: ?scell ( n -- n flag ) dup -2147483648 2147483648 within ;
:  ?char ( n -- n flag ) dup        -128        256 within ;
:  ?half ( n -- n flag ) dup      -32768      65536 within ;
:  ?cell ( n -- n flag ) dup -2147483648 4294967296 within ;

: cleave    ( n u -- n1 n2 ) 2dup invert and -rot and ;
:  rdrop  ( R: n -- )     2r> nip >r ;
: 2rdrop  ( R: n1 n2 -- ) r> 2r> 2drop >r ;
: bounds  ( addr u -- limit index ) over + swap ;

( ---------------------------------------------------------------------------- )
(       String Stuff                                                           )
( ---------------------------------------------------------------------------- )
: ucase    ( c -- c' )   dup [ char a char z 1+ ] 2literal within 32 and - ;
: c=   ( c1 c2 -- flag ) ucase swap ucase =  ;
: c<>  ( c1 c2 -- flag ) ucase swap ucase <> ;
: 1++  ( n1 n2 -- n1' n2' ) 1+ >r 1+ r> ;
: str= ( addr1 u1 addr2 u2 -- flag )
    rot over <> if 3 ndrop false exit endif
    bounds ?do count i c@ c<> if drop unloop false exit endif loop drop true ;
: mem= ( addr1 u1 addr2 u2 -- flag ) compare 0= ;

( ---------------------------------------------------------------------------- )
: .n ( n n' -- )
    ." ." >r s>d <# #s #> r@ min r> over - 0 max 0 ?do ." 0" loop type space ;
: megabytes. ( n -- )
    1 megabytes /mod 0 .r   1000000 $100000 */ 2 .n   ." Megabytes " ;
: kilobytes. ( n -- )
    dup 1 megabytes > if megabytes. exit endif
    1 kilobytes /mod 0 .r   1000 $400  */ 2 .n   ." kilobytes " ;
: bytes. ( n -- ) dup 1 kilobytes > if kilobytes. exit endif   . ." bytes" ;

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


















