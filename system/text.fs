( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Make your Sega Genesis speak English                                   )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - 68k.fs                                                           )
(           - forth.fs                                                         )
(           - core.fs                                                          )
(           - video.fs                                                         )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
(       Glyph Data                                                             )
( ---------------------------------------------------------------------------- )
create glyph-data  bytes[
%00000000 %00000000 %00000000 %00000000 %00000000 %00000000 %00000000 \ [space]
%00111100 %01100110 %01101110 %01111110 %01110110 %01100110 %00111100 \ 0
%00011000 %01111000 %00011000 %00011000 %00011000 %00011000 %01111110 \ 1
%00111100 %01100110 %00000110 %00011100 %00110000 %01100000 %01111110 \ 2
%00111100 %01100110 %00000110 %00011100 %00000110 %01100110 %00111100 \ 3
%00001100 %00011100 %00111100 %01101100 %11001100 %11111110 %00001100 \ 4
%01111110 %01100000 %01111100 %00000110 %00000110 %01100110 %00111100 \ 5
%00111100 %01100110 %01100000 %01111100 %01100110 %01100110 %00111100 \ 6
%01111110 %00000110 %00001100 %00011000 %00011000 %00011000 %00011000 \ 7
%00111100 %01100110 %01100110 %00111100 %01100110 %01100110 %00111100 \ 8
%00111100 %01100110 %01100110 %00111110 %00000110 %01100110 %00111100 \ 9
%01111100 %11000110 %11000110 %11111110 %11000110 %11000110 %11000110 \ A   11
%11111100 %11000110 %11000110 %11111100 %11000110 %11000110 %11111100 \ B   12
%11111110 %11000110 %11000110 %11000000 %11000110 %11000110 %11111110 \ C   13
%11111100 %11000110 %11000110 %11000110 %11000110 %11000110 %11111100 \ D   14
%11111110 %11000000 %11000000 %11111000 %11000000 %11000000 %11111110 \ E   15
%11111110 %11000000 %11000000 %11111000 %11000000 %11000000 %11000000 \ F   16
%11111110 %11000110 %11000000 %11001110 %11000110 %11000110 %11111110 \ G   17
%11000110 %11000110 %11000110 %11111110 %11000110 %11000110 %11000110 \ H   18
%01111110 %00011000 %00011000 %00011000 %00011000 %00011000 %01111110 \ I   19
%00111110 %00001100 %00001100 %00001100 %11001100 %11001100 %11111100 \ J   20
%11000110 %11001110 %11011100 %11111000 %11111100 %11001110 %11000110 \ K   21
%11000000 %11000000 %11000000 %11000000 %11000000 %11000000 %11111110 \ L   22
%11000110 %11101110 %11111110 %11010110 %11010110 %11000110 %11000110 \ M   23
%11000110 %11100110 %11110110 %11111110 %11011110 %11001110 %11000110 \ N   24
%11111110 %11000110 %11000110 %11000110 %11000110 %11000110 %11111110 \ O   25
%11111110 %11000110 %11000110 %11111110 %11000000 %11000000 %11000000 \ P   26
%11111110 %11000110 %11000110 %11000110 %11000110 %11011100 %11101110 \ Q   27
%11111110 %11000110 %11000110 %11111100 %11000110 %11000110 %11000110 \ R   28
%11111110 %11000110 %11000000 %11111110 %00000110 %11000110 %11111110 \ S   29
%01111110 %00011000 %00011000 %00011000 %00011000 %00011000 %00011000 \ T   30
%11000110 %11000110 %11000110 %11000110 %11000110 %11000110 %11111110 \ U   31
%11000110 %11000110 %11000110 %11101110 %01111100 %00111000 %00010000 \ V   32
%11000110 %11000110 %11010110 %11010110 %11111110 %11101110 %11000110 \ W   33
%11000110 %11101110 %01111100 %00111000 %01111100 %11101110 %11000110 \ X   34
%11000110 %11000110 %11101110 %01111100 %00111000 %00111000 %00111000 \ Y   35
%11111110 %00001110 %00011100 %00111000 %01110000 %11100000 %11111110 \ Z   36
%00110000 %00110000 %00110000 %00110000 %00110000 %00000000 %00110000 \ !   37
%00111100 %01100110 %11001110 %11011010 %11011110 %11000000 %01111100 \ @   38
%01101100 %11111110 %01101100 %01101100 %11111110 %01101100 %00000000 \ #   39
%00011000 %01111110 %11011000 %01111110 %00011011 %01111110 %00011000 \ $   40
%11000110 %11001110 %00011100 %00111000 %01110000 %11100110 %11000110 \ %   41
%00010000 %00111000 %01101100 %11000110 %10000010 %00000000 %00000000 \ ^   42
%00111000 %01101100 %00111000 %01111010 %11001110 %11011100 %01110110 \ &   43
%00011000 %11011011 %00111100 %11011011 %00011000 %00000000 %00000000 \ *   44
%00011000 %00110000 %00110000 %00110000 %00110000 %00110000 %00011000 \ (   45
%00011000 %00001100 %00001100 %00001100 %00001100 %00001100 %00011000 \ )   46
%00000000 %00000000 %00000000 %01111110 %00000000 %00000000 %00000000 \ -   47
%00000000 %00011000 %00011000 %01111110 %00011000 %00011000 %00000000 \ +   48
%00000110 %00001110 %00011100 %00111000 %01110000 %11100000 %11000000 \ /   49
%00000000 %00001110 %00111000 %01100000 %00111000 %00001110 %00000000 \ <   50
%00000000 %00000000 %01111110 %00000000 %01111110 %00000000 %00000000 \ =   51
%00000000 %01110000 %00011100 %00000110 %00011100 %01110000 %00000000 \ >   52
%00111100 %01100110 %00000110 %00011100 %00011000 %00000000 %00011000 \ ?   53
%00011000 %00011000 %00010000 %00000000 %00000000 %00000000 %00000000 \ '   54
%01100110 %01100110 %00100100 %00000000 %00000000 %00000000 %00000000 \ "   55
%00000000 %00000000 %00000000 %00000000 %00000000 %00011000 %00011000 \ .   56
%00000000 %00011000 %00011000 %00000000 %00000000 %00011000 %00011000 \ :   57
%00111100 %00110000 %00110000 %00110000 %00110000 %00110000 %00111100 \ [   58
%00011000 %00011000 %00011000 %00011000 %00011000 %00011000 %00011000 \ |   59
%00111100 %00001100 %00001100 %00001100 %00001100 %00001100 %00111100 \ ]   60
%00000000 %00000000 %01110110 %11011100 %00000000 %00000000 %00000000 \ ~   61
%00000000 %00000000 %00000000 %00000000 %00011000 %00011000 %00110000 \ ,   62
%00011000 %00011000 %00000000 %00000000 %00011000 %00011000 %00110000 \ ;   63
]

( ---------------------------------------------------------------------------- )
(       Load Text to Video Memory                                              )
( ---------------------------------------------------------------------------- )
cvalue  text-color-index

rawcode (load-glyph-data) \ A1=video data, A2=glyph data, TOS=color index
    7 d5 do [a2]+ d1 b move, 8 d4 do 1 # d1 b rol, 0 # d1 test-bit, d2 z<> set?,
    tos d2 b and, 4 # d3 lsl, d2 d3 b or, loop d3 [a1] move, loop return, end

code load-glyph-data ( vramaddr -- )
    video-data [#] a1 address, glyph-data [#] a2 address,
    $8F02 # [a1 4 +] h move, 2 # tos lsl, 2 # tos h lsr,
    $4000 # tos or, tos swap, tos [a1 4 +] move,
    text-color-index [#] tos b move, 15 # tos b and, d7 clear,
    62 d6 do (load-glyph-data) subroutine, d7 [a1] move, loop
     2 d6 do d7 [a1] move, (load-glyph-data) subroutine, loop
    tos pull, next

( ---------------------------------------------------------------------------- )
(       ASCII Conversion                                                       )
( ---------------------------------------------------------------------------- )
Host definitions   create ascii-table
 0 hostc, 37 hostc, 55 hostc, 39 hostc, 40 hostc, 41 hostc, 43 hostc, 54 hostc,
45 hostc, 46 hostc, 44 hostc, 48 hostc, 62 hostc, 47 hostc, 56 hostc, 49 hostc,
 1 hostc,  2 hostc,  3 hostc,  4 hostc,  5 hostc,  6 hostc,  7 hostc,  8 hostc,
 9 hostc, 10 hostc, 57 hostc, 63 hostc, 50 hostc, 51 hostc, 52 hostc, 53 hostc,
38 hostc, 11 hostc, 12 hostc, 13 hostc, 14 hostc, 15 hostc, 16 hostc, 17 hostc,
18 hostc, 19 hostc, 20 hostc, 21 hostc, 22 hostc, 23 hostc, 24 hostc, 25 hostc,
26 hostc, 27 hostc, 28 hostc, 29 hostc, 30 hostc, 31 hostc, 32 hostc, 33 hostc,
34 hostc, 35 hostc, 36 hostc, 58 hostc, 59 hostc, 60 hostc, 42 hostc, 53 hostc,
53 hostc, 11 hostc, 12 hostc, 13 hostc, 14 hostc, 15 hostc, 16 hostc, 17 hostc,
18 hostc, 19 hostc, 20 hostc, 21 hostc, 22 hostc, 23 hostc, 24 hostc, 25 hostc,
26 hostc, 27 hostc, 28 hostc, 29 hostc, 30 hostc, 31 hostc, 32 hostc, 33 hostc,
34 hostc, 35 hostc, 36 hostc, 58 hostc, 59 hostc, 60 hostc, 61 hostc, 53 hostc,
: ascii ( c -- c' ) 32 -   0 max 95 min   ascii-table + c@ ;

Forth definitions {
:  string, ( addr u -- ) 0 ?do count ascii c, loop drop ;
: cstring, ( addr u -- ) dup c, string, ;
:  string" ( "text" -- ) [char] " parse  string, ;
: cstring" ( "text" -- ) [char] " parse cstring, ;
}

code  /string ( addr u n ) d1 pull, tos [sp] add, tos d1 sub, d1 tos move, next
code 1/string ( addr u )   1 # [sp] add, tos dec, next

( ---------------------------------------------------------------------------- )
(       Terminal Display                                                       )
( ---------------------------------------------------------------------------- )
hvalue attributes   6 allot
\ cursorY=attributes+2, cursorX=+3, yShift=+4, xMax=+5, vram-addr=+6

code terminal ( vramaddr -- )
    attributes [#] a1 address, video-mode-registers [#] a2 address,
    tos [a1 6 +] h move, tos pull, [a2 17 +] d1 c move,
    3 # d1 c and, 6 # d1 c add, 9 # d1 c compare, d2 z= set?,
    d2 d1 c add, d1 [a1 4 +] c move, [a2 3 +] d1 c move, $81 # d1 c and,
    z<> if 35 # [a1 5 +] c move, else 27 # [a1 5 +] c move, endif next

code terminal-xy ( -- x y )
    attributes [#] a1 address, tos push, tos clear, [a1 3 +] tos c move,
    tos push, [a1 2 +] tos c move, next

code at-xy ( x y -- )
    attributes [#] a1 address, 26 # tos c compare, d1 lt set?, d1 tos c and,
    tos [a1 2 +] c move, tos pull, [a1 5 +] tos c compare, d1 lt set?,
    d1 tos c and, tos [a1 3 +] c move, tos pull, next

code cr ( -- )
    attributes [#] a1 address,   PC: (?cr)   [a1 3 +] c clear,
    [a1 2 +] d1 c move, d1 c inc, 26 # d1 c compare, d2 gt= set?,
    26 # d2 c and, d2 d1 c sub, d1 [a1 2 +] c move, next
code ?cr ( addr u -- )
    attributes [#] a1 address, [a1 5 +] d1 c move, [a1 3 +] d1 c sub,
    d1 tos c compare, (?cr) gt primitive?, next

code (emit3) ( -- )
    attributes [#] a1 address, [a1 3 +] d1 c move, d1 c inc,
    [a1 5 +] d1 c compare, ' cr >body gt primitive?, d1 [a1 3 +] c move, next
code (emit2) ( h vramaddr -- )
    video-data [#] a1 address, tos [a1 4 +] move,
    half # sp add, [sp]+ [a1] h move, tos pull, next
code (emit1) ( c -- h vramaddr )
    attributes [#] a1 address, [a1] tos h add, tos push, tos clear, d1 clear,
    [a1 6 +] tos h move, [a1 2 +] d1 c move, d1 c inc, [a1 4 +] d2 c move,
    d2 d1 h lsl, [a1 3 +] d2 c move, 1 # d2 c lsl, 4 # d2 c add, d2 d1 c add,
    d1 tos h add, 2 # tos lsl, 2 # tos h lsr, $4000 # tos h add, tos swap, next
: <emit> ( c -- ) (emit1) (emit2) (emit3) ;

synonym bl false        defer emit      defer type

: <type> ( addr u -- ) 0 ?do count emit loop drop ;
:  page         ( -- ) 0 0 at-xy ;
:  space        ( -- ) bl emit ;
:  spaces     ( n -- ) 0 max 0 ?do bl emit loop ;
:  blank ( addr u -- ) bl erase ;

: type.r ( addr u u -- ) over - spaces <type> ;

: parse-token ( addr u c -- addr1 u1 addr2 u2 )
    >r begin over c@ r@ = over 0<> and while 1/string repeat
    2dup begin over c@ r@ <> over 0<> and while 1/string repeat
    2swap third - rdrop ;

: <type-words> ( addr u -- )
    begin ?dup while bl parse-token ?cr <type> space repeat drop ;

( ---------------------------------------------------------------------------- )
(       String Words                                                           )
( ---------------------------------------------------------------------------- )
{:} (stringword) ( xt -- ) create , ;code ( -- addr u )
    tos push, tos clear, [tp]+ tos c move, tp push,
    tp d1 move, tos d1 add, d1 inc, -2 # d1 c and, d1 tp move,
    [dfa] dfa move, [dfa]+ a1 move, [a1] jump, end

'noop   (stringword) (s")
' type  (stringword) (.")                

{ : s" ( "text" -- ) comp-only [char] " parse (s") cstring, alignrom ; }
{ : ." ( "text" -- ) comp-only [char] " parse (.") cstring, alignrom ; }

: .flag ( flag -- ) if ." yes " exit endif ." no " ;

( ---------------------------------------------------------------------------- )
(       Number Words                                                           )
( ---------------------------------------------------------------------------- )
code decimal  10 # base [#] move, next
code hex      16 # base [#] move, next

code <# ( -- )
    (numbuffer) [#] a1 address, a1 [a1] move, next       aka <##
code sign ( n -- )
    tos test, neg if (numbuffer) [#] a1 address, [a1] a2 move, a2 dec,
    47 # [a2] c move, a2 [a1] move, endif tos pull, next
code hold ( c -- )
    (numbuffer) [#] a1 address, [a1] a2 move, a2 dec,
    tos [a2] c move, a2 [a1] move, tos pull, next
code #> ( n -- addr u )
    (numbuffer) [#] a1 address, [a1] d1 move,
    d1 push, a1 tos move, d1 tos sub, next
code ##> ( d -- addr u ) cell # sp add, ' #> >body primitive, end

code (##) ( ud flag -- ud' )
    tos d4 move, tos pull, (numbuffer) [#] a1 address,
    [a1] a2 move, a2 dec, base half+ [#] d1 h move, d2 clear,
    tos swap, tos d2 h move, d1 d2 divu, d2 tos h move,
    tos swap, tos d2 h move, d1 d2 divu, d2 tos h move, d4 test, z<> if    
        d3 pull, d3 swap,  d3 d2 h move, d1 d2 divu, d2  d3 h move,
                 d3 swap,  d3 d2 h move, d1 d2 divu, d2  d3 h move, d3 push,
    endif d2 swap, d2 c inc, d2 [a2] c move, a2 [a1] move, next
:  #  ( u -- u' ) false (##) ;
: ## ( ud -- ud' ) true (##) ;

:  #s ( n -- 0  ) begin  #  dup  0= until ;
: ##s ( d -- 0d ) begin ## 2dup d0= until ;

:   string  ( n -- addr u )  dup  abs  <#  #s swap sign #> ;
:  dstring  ( d -- addr u ) tuck dabs <## ##s  rot sign ##> ;
:  ustring  ( u -- addr u )  <#  #s #> ;
: udstring ( ud -- addr u ) <## ##s ##> ;

:   .  ( n -- )   string type space ;
:  u.  ( u -- )  ustring type space ;
:  d.  ( d -- )  dstring type space ;
: ud. ( ud -- ) udstring type space ;

:   .r  ( n u -- ) >r   string r> type.r ;
:  u.r  ( u u -- ) >r  ustring r> type.r ;
:  d.r  ( d u -- ) >r  dstring r> type.r ;
: ud.r ( ud u -- ) >r udstring r> type.r ;

:  hex. ( u )  base @ >r hex <# #s 40 hold #> type space r> base ! ;
: dhex. ( ud ) base @ >r ." $" hex swap <# #s drop #s #> type space r> base ! ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

















