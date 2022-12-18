( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Core Word Set                                                          )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - romfile.fs                                                       )
(           - 68k.fs                                                           )
(           - forth.fs                                                         )
(                                                                              )
(       TODO:                                                                  )
(           - move                                                             )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
(       State-Smart Words                                                      )
( ---------------------------------------------------------------------------- )
-1 constant true
 0 constant false
 4 constant cell
 2 constant half
 1 constant byte

{ '  + }    anon          [sp]+ tos add, next   host/target:  +
{ '  - }    anon tos neg, [sp]+ tos add, next   host/target:  -
{ ' 1+ }               anon     tos inc, next   host/target: 1+     aka char+
{ ' 1- }               anon     tos dec, next   host/target: 1-     aka char-
{ ' 2* }               anon 1 # tos lsl, next   host/target: 2*     aka halves
{ ' 2/ }               anon 1 # tos asr, next   host/target: 2/     aka half/
{ :noname 2* 2*    ; } anon 2 # tos lsl, next   host/target: 4*     aka cells
{ :noname 2/ 2/    ; } anon 2 # tos asr, next   host/target: 4/     aka cell/
{ :noname 2* 2* 2* ; } anon 3 # tos lsl, next   host/target: 8*
{ :noname 2/ 2/ 2/ ; } anon 3 # tos asr, next   host/target: 8/
{ :noname 2 + ; }      anon 2 # tos add, next   host/target: 2+     aka half+
{ :noname 2 - ; }      anon 2 # tos sub, next   host/target: 2-     aka half-
{ :noname 4 + ; }      anon 4 # tos add, next   host/target: 4+     aka cell+
{ :noname 4 - ; }      anon 4 # tos sub, next   host/target: 4-     aka cell-
{ :noname 8 + ; }      anon 8 # tos add, next   host/target: 8+
{ :noname 8 - ; }      anon 8 # tos sub, next   host/target: 8-

{ '  abs }      anon tos test, neg if tos neg, endif next   host/target: abs
{ ' lshift }    anon d1 pull, tos d1 lsl, d1 tos move, next host/target: lshift
{ ' rshift }    anon d1 pull, tos d1 lsr, d1 tos move, next host/target: rshift
{ ' invert }    anon tos not, next                          host/target: invert
{ ' negate }    anon tos neg, next                          host/target: negate
{ ' and }       anon [sp]+ tos and, next                    host/target: and
{ '  or }       anon [sp]+ tos  or, next                    host/target:  or
{ ' xor }       anon d1 pull, d1 tos xor, next              host/target: xor

{ ' swap }      anon d1 pull, tos push, d1 tos move, next   host/target: swap
{ ' drop }      anon tos pull, next                         host/target: drop
{ '  dup }      anon tos push, next                         host/target:  dup
{ ' ?dup }      anon tos test, z<> if tos push, endif next  host/target: ?dup
{ ' over }      anon d1 peek, tos push, d1 tos move, next   host/target: over
{ '  nip }      anon cell # sp add, next                    host/target:  nip
{ ' tuck }      anon d1 pull, tos push, d1 push, next       host/target: tuck

{ '  rot }      anon d1 pull, d2 pull, d1 push,
                    tos push, d2 tos move, next     host/target:  rot
{ ' -rot }      anon d1 pull, d2 pull, tos push, 
                     d2 push, d1 tos move, next     host/target: -rot
{ ' pick }      anon 2 # tos lsl,
                    [sp tos+ 0] tos move, next      host/target: pick
{ ' roll }
    anon tos test, z= if tos pull, next, endif
        tos d1 move, 2 # tos lsl,
        [sp tos+ 4 +] a1 address, [a1 -4 +] a2 address,
        [a2] tos move, d6 begin -[a2] -[a1] move, loop 4 # sp add, next
                                                    host/target: roll

synonym >body cell+

{ :noname 1+ -2 and ; } anon tos inc, -2 # tos b and, next host/target: aligned

create donoop& PC: 'noop
{ :noname ; }   anon next   host/target: noop       aka chars   aka bytes

{ :noname 10 lshift ; }
anon 10 # d1 move, d1 tos lsl, next                 host/target: kilobytes

( ---------------------------------------------------------------------------- )
(       Dumb Words  [aka "compile-only"]                                       )
( ---------------------------------------------------------------------------- )
code   @        tos a1 move, [a1] tos move, next
code  h@        tos a1 move, tos clear, [a1] tos h move, next
code  c@        tos a1 move, tos clear, [a1] tos c move, next
code sh@        tos a1 move, [a1] tos h move, tos   extend, next
code sc@        tos a1 move, [a1] tos c move, tos c extend, next
code   !        tos a1 move, [sp]+ [a1] move, tos pull, next
code  h!        tos a1 move, 2 # sp add, [sp]+ [a1] h move, tos pull, next
code  c!        tos a1 move, 3 # sp add, [sp]+ [a1] c move, tos pull, next
code  2@        tos a1 move, [a1]+ tos move, [a1] -[sp] move, next
code  2!        tos a1 move, [sp]+ [a1]+ move, [sp]+ [a1]+ move, tos pull, next
code  +!        tos a1 move, d1 pull, d1 [a1]   add, tos pull, next
code h+!        tos a1 move, d1 pull, d1 [a1] h add, tos pull, next
code c+!        tos a1 move, d1 pull, d1 [a1] c add, tos pull, next
code 2+!        tos a1 move, d1 pull, d2 pull, [a1] d3 move,
                d2 [a1 cell +] add, d1 d3 addx, d3 [a1] move, tos pull, next

code on         tos a1 move, -1 # tos move, tos [a1] move, tos pull, next
code off        tos a1 move,  0 # tos move, tos [a1] move, tos pull, next

code count      tos a1 move, tos clear, [a1]+ tos c move, a1 push, next
code halfcount  tos a1 move, tos clear, [a1]+ tos h move, a1 push, next
code cellcount  tos a1 move, [a1]+ tos move, a1 push, next

code 2drop      cell # sp add, tos pull, next
code 2dup       d1 peek, tos push, d1 push, next
code 2over      tos push, [sp 2 cells +] tos move, [sp 3 cells +] push, next
code 2nip       d1 pull, 2 cells # sp add, d1 push, next
code 2tuck      [sp]+ [[ d1 d2 d3 ]] movem, d1 push, tos push,
                [[ d1 d2 d3 ]] -[sp] movem, next
code 2swap      d1 pull, d2 pull, d3 pull, d1 push,
                tos push, d3 push, d2 tos move, next
code third      tos push, [sp 2 cells +] tos move, next
code fourth     tos push, [sp 3 cells +] tos move, next

code  >r        tos rpush, tos  pull, next
code  r>        tos  push, tos rpull,  next
code 2>r        [sp]+ rpush, tos rpush, tos pull, next
code 2r@        tos push, tos rpeek, [rp cell +] push, next
code 2r>        tos push, tos rpull, -[sp] rpull, next
code  i         tos push, tos rpeek, next                   aka r@
code  i'        tos push, [rp   cell  +] tos move, next     aka rsecond
code  j         tos push, [rp 2 cells +] tos move, next     aka rthird
code  k         tos push, [rp 4 cells +] tos move, next
code  rdrop       cell  # rp add, next
code 2rdrop     2 cells # rp add, next                      aka unloop
code  ndrop     2 # tos lsl, tos sp add, tos pull, next

code dinvert    [sp] not, tos not,  next
code dnegate    [sp] neg, tos negx, next
code dabs       tos test, neg if [sp] neg, tos negx, endif next
code d2*        [sp] lsl, 1 # tos roxl, next
code d2/        1 # tos asr, [sp] roxr, next
code d+         d1 pull, d2 pull, [sp]+ d1 add, d2 tos addx, d1 push, next
code d-         d1 pull, d2 pull, d1 neg, tos negx,
                [sp]+ d1 add, d2 tos addx, d1 push, next

code arshift    d1 pull, tos d1   asr, d1 tos move, next
code lrotate    d1 pull, tos d1   rol, d1 tos move, next
code rrotate    d1 pull, tos d1   ror, d1 tos move, next
code lhrotate   d1 pull, tos d1 h rol, d1 tos move, next
code rhrotate   d1 pull, tos d1 h ror, d1 tos move, next
code lcrotate   d1 pull, tos d1 c rol, d1 tos move, next
code rcrotate   d1 pull, tos d1 c ror, d1 tos move, next

code dlshift
    31 # tos compare, gt if 32 # tos sub, 4 # sp add, d1 peek,
        tos d1 lsl, [sp] clear, d1 tos move, next, endif
    d1 pull, d2 pull, -1 # d3 move, tos d1 lsl, tos d2 rol, tos d3 lsl,
    d3 tos move, tos not, d2 tos and, d1 tos or, d3 d2 and, d2 push, next
code drshift
    31 # tos compare, gt if 32 # tos sub, d1 pull, 4 # sp add,
        tos d1 lsr, d1 push, tos clear, next, endif
    d1 pull, d2 pull, -1 # d3 move, tos d1 ror, tos d2 lsr, tos d3 lsr,
    d3 tos move, d1 tos and, d3 not, d3 d1 and, d1 d2 or, d2 push, next

code 16lshift   tos swap, tos h clear, next
code 16rshift   tos h clear, tos swap, next
code 16rotate   tos swap, next                  synonym 32drotate swap

code min        d1 pull, d1 tos compare, gt if d1 tos move, endif next
code max        d1 pull, d1 tos compare, lt if d1 tos move, endif next

code 0=         tos test, tos  z=  set?, tos c extend, next   aka not
code 0<>        tos test, tos  z<> set?, tos c extend, next   aka flag
code 0<         tos test, tos  lt  set?, tos c extend, next
code 0<=        tos test, tos  lt= set?, tos c extend, next
code 0>         tos test, tos  gt  set?, tos c extend, next
code 0>=        tos test, tos  gt= set?, tos c extend, next
code   =        [sp]+ tos compare, tos  z=  set?, tos c extend, next
code  <>        [sp]+ tos compare, tos  z<> set?, tos c extend, next
code  >         [sp]+ tos compare, tos  lt  set?, tos c extend, next
code  >=        [sp]+ tos compare, tos  lt= set?, tos c extend, next
code  <         [sp]+ tos compare, tos  gt  set?, tos c extend, next
code  <=        [sp]+ tos compare, tos  gt= set?, tos c extend, next
code u>         [sp]+ tos compare, tos ult  set?, tos c extend, next
code u>=        [sp]+ tos compare, tos ult= set?, tos c extend, next
code u<         [sp]+ tos compare, tos ugt  set?, tos c extend, next
code u<=        [sp]+ tos compare, tos ugt= set?, tos c extend, next

code  d0=       [sp]+ tos or, tos z=  set?, tos c extend, next
code  d0<>      [sp]+ tos or, tos z<> set?, tos c extend, next
code  d0<       tos test, tos  lt  set?, tos c extend, cell # sp add, next
code  d0>=      tos test, tos  gt= set?, tos c extend, cell # sp add, next
code  d=        d1 pull, [sp]+ tos sub, [sp]+ d1 sub,
                d1 tos or, tos z=  set?, tos c extend, next
code  d<>       d1 pull, [sp]+ tos sub, [sp]+ d1 sub,
                d1 tos or, tos z<> set?, tos c extend, next
code  d<        d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos  lt  set?, tos c extend, next
code  d<=       d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos  lt= set?, tos c extend, next
code  d>        d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos  gt  set?, tos c extend, next
code  d>=       d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos  gt= set?, tos c extend, next
code ud<        d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos ult  set?, tos c extend, next
code ud<=       d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos ult= set?, tos c extend, next
code ud>        d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos ugt  set?, tos c extend, next
code ud>=       d1 pull, d2 pull, d3 pull, d1 d3 sub,
                tos d2 subx, tos ugt= set?, tos c extend, next

code under+     tos [sp cell +] add, tos pull, next
code under-     tos [sp cell +] sub, tos pull, next

( ---------------------------------------------------------------------------- )
(       Block Operations                                                       )
( ---------------------------------------------------------------------------- )
code cmove ( source dest length -- )
    a2 pull, a1 pull, tos h dec, pos if
    tos begin [a1]+ [a2]+ c move, loop endif tos pull, next

code cmove> ( source dest length -- )
    a2 pull, tos a2 h add, a1 pull, tos a1 h add, tos h dec, pos if
    tos begin -[a1] -[a2] c move, loop endif tos pull, next

code move ( source dest length -- )
    tos test, z= if 2 cells # sp add, tos pull, next, endif
    a2 pull, a1 pull, a2 d2 h move, a1 d1 h move, d2 d1 h xor,
    0 # d1 test-bit, z<> if
        a1 a2 compare, ult if tos h dec, tos begin [a1]+ [a2]+ c move, loop
        else tos a1 h add, tos a2 h add,
             tos h dec, tos begin -[a1] -[a2] c move, loop
        endif tos pull, next, endif
    a1 a2 compare, ult if
        0 # d2 test-bit, z<> if [a1]+ [a2]+ c move, tos dec, endif
        2 # tos ror, tos h dec, pos if tos begin [a1]+ [a2]+ move, loop endif
        1 # tos rol, cset if [a1]+ [a2]+ h move, endif
        1 # tos rol, cset if [a1]+ [a2]+ c move, endif
    else
        tos a1 add, tos a2 add, a2 d2 h move,
        0 # d2 test-bit, z<> if -[a1] -[a2] c move, tos dec, endif
        2 # tos ror, tos h dec, pos if tos begin -[a1] -[a2] move, loop endif
        1 # tos rol, cset if -[a1] -[a2] h move, endif
        1 # tos rol, cset if -[a1] -[a2] c move, endif
    endif tos pull, next

code fill ( addr length c -- )
    d1 pull, z= if cell # sp add, tos pull, next, endif
    a1 pull, a1 d2 move, 0 # d2 test-bit, z<> if tos [a1]+ c move, d1 dec, endif
    tos d2 c move, tos c rpush, tos h rpull,
    d2 tos c move, tos d2 h move, tos swap, d2 tos h move,
    2 # d1 ror, d1 h dec, pos if d1 begin tos [a1]+ move, loop endif
    1 # d1 rol, cset if tos [a1]+ h move, endif
    1 # d1 rol, cset if tos [a1]+ c move, endif tos pull, next

: erase ( addr length -- ) 0 fill ;

( ---------------------------------------------------------------------------- )
(       Flow Control Words                                                     )
( ---------------------------------------------------------------------------- )
{ : (disp?) ( n -- )    ?shalf not abort" Destination out of range." ; }
{ : (disp)  ( u -- n )  romspace      - 2 - (disp?) ; }
{ : (disp>) ( u -- n )  romspace swap - 2 - (disp?) ; }
{ : (orig)  ( -- orig ) romspace 0 h, ; }
{ : (orig>) ( orig -- ) ?dup if dup (disp>) swap romh! endif ; }
{ : (dest)  ( -- dest ) romspace ; }
{ : (>dest) ( dest -- ) (disp) h, ; }

code  branch    [tp]+ tp h add, next
code 0branch    d1 h read, tos test, z= if d1 tp h add, endif tos pull, next
code ?branch    d1 h read, tos test, z= if d1 tp h add, tos pull, endif next

{ :  if   ( -- orig ) comp-only 0branch (orig) ; }
{ : ?if   ( -- orig ) comp-only ?branch (orig) ; }
{ : ahead ( -- orig ) comp-only  branch (orig) ; }
{ : then  ( orig -- ) comp-only (orig>) ; }         aka endif

{ : begin ( -- dest ) comp-only (dest) ; }
{ : again ( dest -- ) comp-only  branch (>dest) ; }
{ : until ( dest -- ) comp-only 0branch (>dest) ; }

{ : else  ( orig1 -- orig2 )     comp-only  branch  (orig)  swap  (orig>) ; }
{ : while  ( dest -- orig dest ) comp-only 0branch  (orig)  swap   ; }
{ : repeat ( orig dest -- )      comp-only  branch (>dest) (orig>) ; }

( ---------------------------------------------------------------------------- )
code (of)       d1 h read, d2 pull, d2 tos compare,
                z<> if d1 tp h add, d2 tos move, next, endif tos pull, next
code (of?)      d1 h read, tos test, z= if d1 tp h add, tos pull, next, endif 
                4 # sp add, tos pull, next

{ : case        comp-only 0 ; }
{ : of          comp-only (of)  (orig) ; }
{ : of?         comp-only (of?) (orig) ; }
{ : endof       comp-only >r >r branch (orig) r> 1+ r> (orig>) ; }
{ : endcase     comp-only } drop { 0 ?do (orig>) loop ; }

( ---------------------------------------------------------------------------- )
code   (do)     [sp]+ -[rp] move, tos rpush, tos pull, next
code  (?do)     d1 h read, [sp] tos compare, ' (do) >body z<> primitive?,
                d1 tp h add, 4 # sp add, tos pull, next
code  (loop)    d1 h read, d2 rpull, d2 inc, [rp] d2 compare,
                z<> if d1 tp h add, d2 rpush, next, endif 4 # rp add, next
code (+loop)    d1 h read, d2 rpull, d3 rpeek, d3 d2 compare, d4 neg set?,
                tos d2 add, d3 d2 compare, tos neg set?, tos d4 b compare,
                z= if d1 tp h add, d2 rpush, tos pull, next, endif
                cell # rp add, tos pull, next
code (-loop)    d1 h read, d2 rpull, d3 rpeek, d2 d3 compare, d4 neg set?,
                tos d2 sub, d2 d3 compare, tos neg set?, tos d4 b compare,
                z= if d1 tp h add, d2 rpush, tos pull, next, endif
                cell # rp add, tos pull, next
code (leave)    2 cells # rp add, [tp]+ tp h add, next
code  culminate [rp cell +] d1 move, d1 dec, d1 [rp] move, next  aka +culminate
code -culminate [rp cell +] d1 move, d1 inc, d1 [rp] move, next
code /culminate [rp cell +] [rp] move, next

Host definitions
variable dumb dumb dumb !       25 cells allot
: dumb+ dumb @ cell+ dup dumb ! ! ; : dumb- dumb @ dup cell- dumb ! @ ;
: dumb? dumb @ dumb u> ; : dumb! begin dumb? while dumb- (orig>) repeat ;

Forth definitions
{ :   do   ( --  0   dest ) comp-only   (do)    0     (dest)  ; }
{ :  ?do   ( -- orig dest ) comp-only  (?do)   (orig) (dest)  ; }
{ :  loop  ( orig dest -- ) comp-only  (loop) (>dest) (orig>) dumb! ; }
{ : +loop  ( orig dest -- ) comp-only (+loop) (>dest) (orig>) dumb! ; }
{ : -loop  ( orig dest -- ) comp-only (-loop) (>dest) (orig>) dumb! ; }
{ :  leave  ( L: -- orig )  comp-only (leave)  (orig)  dumb+  ; }

( ---------------------------------------------------------------------------- )
(       Records                                                                )
( ---------------------------------------------------------------------------- )
{ : record ( -- off ) host-only 0 ; }

{ : begin-structure ( "name" -- addr off ) host-only 0 pconstant PC half- 0 ; }
{ :   end-structure ( addr off -- )        host-only swap romh! ; }

create dofield&     asm d1 clear, [dfa] d1 h move, d1 tos add, next
{ : +field ( off size "name" -- off' ) host-only
[ Optimization ] [IF]
    create over if dofield& codefield, over h,rom,
              else donoop&  codefield, 0 host, endif +
    does> ( n -- n' )
        compiling? if dup cell+ @ if @ , exit endif drop exit endif
        cell+ @ + ; }
[ELSE]
    create dofield& codefield, over h,rom, +
    does> ( n -- n' ) compiling? if @ , exit endif cell+ @ + ; }
[THEN]

{ :  field: ( off "name" -- off' ) host-only } aligned   cell  +field { ; }
{ : hfield: ( off "name" -- off' ) host-only } aligned   half  +field { ; }
{ : cfield: ( off "name" -- off' ) host-only }           byte  +field { ; }
{ : 2field: ( off "name" -- off' ) host-only } aligned 2 cells +field { ; }

( ---------------------------------------------------------------------------- )
(       Stack Accessors                                                        )
( ---------------------------------------------------------------------------- )
code sp@    tos push, sp tos move, next
code sp!    tos sp move, tos pull, next
code rp@    tos push, rp tos move, next
code rp!    tos rp move, tos pull, next
code tp@    tos push, tp tos move, next
code tp!    tos tp move, tos pull, next
code np@    tos push, np tos move, next
code np!    tos np move, tos pull, next

alignram ReturnStackSize buffer: (rp-limit)     0 buffer: (rp-empty)

code rdepth tos push, (rp-empty) # tos move, rp tos sub, 2 # tos asr, next

( ---------------------------------------------------------------------------- )
(       Exceptions                                                             )
( ---------------------------------------------------------------------------- )
ExceptionStackSize buffer: (estack-limit)   hvariable (estack)

rawcode (throw) \ TOS=throw code (must be non-zero)
    (estack) [#] a3 address, -1 # d1 move, [a3] d1 h move, d1 a1 move,
    [a1]+ d1 h move, d1 sp move, [a1]+ d1 h move, d1 rp move,
    a1 [a3] h move, tp rpull, next

Assembler definitions  { : throw-primitive,  (throw) primitive, ; }
Forth definitions

code (epush) ( -- )
    (estack) [#] a3 address, -1 # d1 move, [a3] d1 h move, 
    DEBUG [IF]
        (estack-limit) $FFFF and # d1 h compare,
        ult= if tos push, -53 # tos move, throw-primitive, endif
    [THEN]
    d1 a1 move, rp -[a1] h move, sp -[a1] h move, a1 [a3] h move, next
code (epop) ( -- )
    (estack) [#] a3 address, -1 # d1 move, [a3] d1 h move, d1 a1 move,
    DEBUG [IF]
        [a1] d1 h move, sp [a1]+ h move, d1 sp move,
        [a1] d1 h move, rp [a1]+ h move, d1 rp move,
    [ELSE] [a1]+ d1 h move, d1 sp move, [a1]+ d1 h move, d1 rp move, [THEN]
    a1 [a3] h move, next
code (edrop) ( -- ) (estack) [#] a3 address, cell # [a3] h add, next

: catch ( xt -- ) (epush) execute (edrop) 0 ;
: throw  ( n -- ) ?if (epop) endif ;

: init-exceptions ( -- ) [ (estack) $FFFF and ] literal (estack) h! ;

DEBUG [IF]
    PC (docolon-vector) 2 - romh!
    rawcode (docolon&)
        rp d1 move, (rp-limit) $FFFF and # d1 h compare,
        ult= if tos push, -5 # tos move, throw-primitive, endif
        tp rpush, dfa tp move, next
    code (eptrs) ( -- sp rp )
        (estack) [#] a3 address, -1 # d1 move, [a3] d1 h move, d1 a1 move,
        tos push, [a1 -4 +] d1 h move, d1 push,
        [a1 -2 +] d1 h move, d1 tos move, next
[THEN]

( ---------------------------------------------------------------------------- )
(       Memory Regions                                                         )
( ---------------------------------------------------------------------------- )
\ numeric output string buffer
variable base   40 allot variable (numbuffer)

\ data stack
StackSize buffer: (sp-limit) 0 buffer: (sp-empty)
code depth ( -- n )
    sp d1 move, tos push, (sp-empty) # tos move, d1 tos sub, 2 # tos asr, next

\ float stack
FloatStack [IF]
    FloatStackSize buffer: (fp-limit) 0 buffer: (fp-empty)
    code fdepth tos push, (fp-empty) # tos move, fp tos sub, 2 # tos asr, next
    code fp@    tos push, fp tos move, next
    code fp!    tos fp move, tos pull, next
[THEN]

( ---------------------------------------------------------------------------- )
(       Other Words                                                            )
( ---------------------------------------------------------------------------- )
code   mux  tos d1 move, [sp]+ tos and, d1 not, [sp]+ d1 and, d1 tos or, next
code demux  tos d1 move, [sp]  tos and, d1 not, d1 [sp]  and, next

( ---------------------------------------------------------------------------- )
code bounds d1 peek, tos [sp] add, d1 tos move, next

code within d1 pull, d1 tos sub, d2 pull, d1 d2 sub,
            d2 tos compare, tos ugt set?, tos c extend, next

( ---------------------------------------------------------------------------- )
code ms ( n -- )
    tos test, gt if tos begin 708 d1 do loop loop endif tos pull, next

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
















