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
(           - multiplication and division                                      )
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
{ ' lshift }    anon d1 pop, tos d1 lsl, d1 tos move, next  host/target: lshift
{ ' rshift }    anon d1 pop, tos d1 lsr, d1 tos move, next  host/target: rshift
{ ' invert }    anon tos not, next                          host/target: invert
{ ' negate }    anon tos neg, next                          host/target: negate
{ ' and }       anon [sp]+ tos and, next                    host/target: and
{ '  or }       anon [sp]+ tos  or, next                    host/target:  or
{ ' xor }       anon d1 pop, d1 tos xor, next               host/target: xor

{ ' swap }      anon d1 pop, tos push, d1 tos move, next    host/target: swap
{ ' drop }      anon tos pop, next                          host/target: drop
{ '  dup }      anon tos push, next                         host/target:  dup
{ ' ?dup }      anon tos test, z<> if tos push, endif next  host/target: ?dup
{ ' over }      anon d1 peek, tos push, d1 tos move, next   host/target: over
{ '  nip }      anon cell # sp add, next                    host/target:  nip
{ ' tuck }      anon d1 pop, tos push, d1 push, next        host/target: tuck

{ '  rot }      anon d1 pop, d2 pop, d1 push,
                    tos push, d2 tos move, next     host/target:  rot
{ ' -rot }      anon d1 pop, d2 pop, tos push, 
                     d2 push, d1 tos move, next     host/target: -rot
{ ' pick }      anon 2 # tos lsl,
                    [sp tos+ 0] tos move, next      host/target: pick
{ ' roll }
    anon tos test, z= if tos pop, next, endif
        tos d1 move, 2 # tos lsl, [sp tos+ 4 +] a1 lea, [a1 -4 +] a2 lea,
        [a2] tos move, d6 begin -[a2] -[a1] move, loop 4 # sp add, next
                                                    host/target: roll

{ :noname 1+ -2 and ; } anon tos inc, -2 # tos b and, next host/target: aligned

PC: 'noop
{ :noname ; }   anon next   host/target: noop       aka chars   aka bytes

{ :noname 10 lshift ; }
anon 10 # d1 move, d1 tos lsl, next                 host/target: kilobytes

create donoop& asm next

( ---------------------------------------------------------------------------- )
{ ' * } anon
    d1 pop, d1 d2 h move, tos d2 mulu, d1 d3 h move, d1 swap,
    d1 h test, z<> if tos d1 mulu, endif d1 swap, d1 h clear, d2 d1 add,
    tos swap, tos h test, z<> if d3 tos mulu, endif tos swap, tos h clear,
    d1 tos add, next
host/target: *

( ---------------------------------------------------------------------------- )
(       Dumb Words  [aka "compile-only"]                                       )
( ---------------------------------------------------------------------------- )
code   @        tos a1 move, [a1] tos move, next
code  h@        tos a1 move, tos clear, [a1] tos h move, next
code  c@        tos a1 move, tos clear, [a1] tos c move, next
code sh@        tos a1 move, [a1] tos h move, tos ext, next
code sc@        tos a1 move, [a1] tos c move, tos h ext, tos ext, next
code   !        tos a1 move, [sp]+ [a1] move, tos pop, next
code  h!        tos a1 move, 2 # sp add, [sp]+ [a1] h move, tos pop, next
code  c!        tos a1 move, 3 # sp add, [sp]+ [a1] c move, tos pop, next
code  2@        tos a1 move, [a1]+ tos move, [a1] -[sp] move, next
code  2!        tos a1 move, [sp]+ [a1]+ move, [sp]+ [a1]+ move, tos pop, next
code  +!        tos a1 move, d1 pop, d1 [a1]   add, tos pop, next
code h+!        tos a1 move, d1 pop, d1 [a1] h add, tos pop, next
code c+!        tos a1 move, d1 pop, d1 [a1] c add, tos pop, next
code 2+!        tos a1 move, d1 pop, d2 pop, [a1] d3 move,
                d2 [a1 cell +] add, d1 d3 addx, d3 [a1] move, tos pop, next

code on         tos a1 move, -1 # tos move, tos [a1] move, tos pop, next
code off        tos a1 move,  0 # tos move, tos [a1] move, tos pop, next

code count      tos a1 move, tos clear, [a1]+ tos c move, a1 push, next
code halfcount  tos a1 move, tos clear, [a1]+ tos h move, a1 push, next
code cellcount  tos a1 move, [a1]+ tos move, a1 push, next

code 2drop      cell # sp add, tos pop, next
code 2dup       d1 peek, tos push, d1 push, next
code 2over      tos push, [sp 2 cells +] tos move, [sp 3 cells +] push, next
code 2nip       d1 pop, 2 cells # sp add, d1 push, next
code 2tuck      [sp]+ [[ d1 d2 d3 ]] movem, d1 push, tos push,
                [[ d1 d2 d3 ]] -[sp] movem, next
code 2swap      d1 pop, d2 pop, d3 pop, d1 push,
                tos push, d3 push, d2 tos move, next
code third      tos push, [sp 2 cells +] tos move, next
code fourth     tos push, [sp 3 cells +] tos move, next

code  >r        tos rpush, tos  pop,  next
code  r>        tos  push, tos rpop,  next
code 2>r        [sp]+ rpush, tos rpush, tos pop, next
code 2r@        tos push, tos rpeek, [rp cell +] push, next
code 2r>        tos push, tos pop, -[sp] rpop, next
code  i         tos push, tos rpeek, next                   aka r@
code  i'        tos push, [rp   cell  +] tos move, next     aka rsecond
code  j         tos push, [rp 2 cells +] tos move, next     aka rthird
code  k         tos push, [rp 4 cells +] tos move, next
code  rdrop       cell  # rp add, next
code 2rdrop     2 cells # rp add, next                      aka unloop
code  ndrop     2 # tos lsl, tos sp add, tos pop, next

code dinvert    [sp] not, tos not,  next
code dnegate    [sp] neg, tos negx, next
code dabs       tos test, neg if [sp] neg, tos negx, endif next
code d2*        [sp] lsl, 1 # tos roxl, next
code d2/        1 # tos asr, [sp] roxr, next
code d+         d1 pop, d2 pop, [sp]+ d1 add, d2 tos addx, d1 push, next
code d-         d1 pop, d2 pop, d1 neg, tos neg,
                [sp]+ d1 add, d2 tos addx, d1 push, next
(
code um*        
    d1 d7 move, d1 d6 mulu, d6 swap, d2 d6 w move, d2 d7 mulu, d7 swap,
    d0 d6 w move, d0 d7 mulu, d6 swap, d3 d6 w move, d3 d7 mulu,
    d6 -1 # move, d6 w clear, d6 swap,
    d3 swap, d4 d3 move, d4 w clear, d3 d6 and, d1 d4 add, d0 d3 addx,
    d2 swap, d4 d2 move, d4 w clear, d2 d6 and, d1 d4 add, d0 d2 addx,
    return, endcode

code um*old     tos d1 h move, tos swap, d2 pop, d2 d3 h move, d2 swap,
                \ tos=xH, d1=xL, d2=xH, d3=xL
                d1 d4 h move,
                \ tos=xH, d1=xL, d2=xH, d3=xL, d4=xL
                d3 d4 mulu, tos d3 mulu, d2 d1 mulu, tos d2 mulu,
                d1 swap, d3 swap,
                \ d1=LH, d2=HH, d3=LH, d4=LL
                tos clear, d1 tos h move, d1 h clear,
                \ tos=0H, d1=L0, d2=HH, d3=LH, d4=LL

                d5 clear, d3 d5 h move, d3 h clear,
                \ tos=0H, d1=L0, d2=HH, d3=L0, d4=LL, d5=0H
                d3 d1 add, d5 tos addx,
                \ tos=0H', d1=L'0, d2=HH, d4=LL
                d4 d1 add, d2 tos addx, d1 push, next
                \ tos=HH, d1=LL
)

code h*         2 # sp add, [sp]+ tos h muls, next
code h/         d1 pop, tos d1 divs, d1 tos h move, tos ext, next
code hmod       d1 pop, tos d1 divs, d1 swap, d1 tos h move, tos ext, next
code h/mod      d1 pop, tos d1 divs, d1 tos h move, tos ext,
                d1 swap, d1 ext, d1 push, next
code uh*        2 # sp add, [sp]+ tos h mulu, next
code uh/        d1 pop, tos d1 divu, tos clear, d1 tos h move, next
code uhmod      d1 pop, tos d1 divs, d1 swap, tos clear, d1 tos h move, next
code uh/mod     d1 pop, tos d1 divs, tos clear, d1 tos h move,
                d1 h clear, d1 swap, d1 push, next

code arshift    d1 pop, tos d1   asr, d1 tos move, next
code lrotate    d1 pop, tos d1   rol, d1 tos move, next
code rrotate    d1 pop, tos d1   ror, d1 tos move, next
code lhrotate   d1 pop, tos d1 h rol, d1 tos move, next
code rhrotate   d1 pop, tos d1 h ror, d1 tos move, next
code lcrotate   d1 pop, tos d1 c rol, d1 tos move, next
code rcrotate   d1 pop, tos d1 c ror, d1 tos move, next

code dlshift
    31 # tos comp, gt if 32 # tos sub, 4 # sp add, d1 peek,
        tos d1 lsl, [sp] clear, d1 tos move, next, endif
    d1 pop, d2 pop, -1 # d3 move, tos d1 lsl, tos d2 rol, tos d3 lsl,
    d3 tos move, tos not, d2 tos and, d1 tos or, d3 d2 and, d2 push, next
code drshift
    31 # tos cmp, gt if 32 # tos sub, d1 pop, 4 # sp add,
        tos d1 lsr, d1 push, tos clear, next, endif
    d1 pop, d2 pop, -1 # d3 move, tos d1 ror, tos d2 lsr, tos d3 lsr,
    d3 tos move, d1 tos and, d3 not, d3 d1 and, d1 d2 or, d2 push, next

code 16rotate   tos swap, next          synonym 32drotate swap

code umin       d1 pop, d1 tos comp, ugt if d1 tos move, endif next
code umax       d1 pop, d1 tos comp, ult if d1 tos move, endif next
code  min       d1 pop, d1 tos comp,  gt if d1 tos move, endif next
code  max       d1 pop, d1 tos comp,  lt if d1 tos move, endif next

code 0=               tos test, tos  z=  set?, tos w ext, tos ext, next aka not
code 0<>              tos test, tos  z<> set?, tos w ext, tos ext, next aka flag
code 0<               tos test, tos  lt  set?, tos w ext, tos ext, next
code 0<=              tos test, tos  lt= set?, tos w ext, tos ext, next
code 0>               tos test, tos  gt  set?, tos w ext, tos ext, next
code 0>=              tos test, tos  gt= set?, tos w ext, tos ext, next
code   =        [sp]+ tos comp, tos  z=  set?, tos w ext, tos ext, next
code  <>        [sp]+ tos comp, tos  z<> set?, tos w ext, tos ext, next
code  >         [sp]+ tos comp, tos  lt  set?, tos w ext, tos ext, next
code  >=        [sp]+ tos comp, tos  lt= set?, tos w ext, tos ext, next
code  <         [sp]+ tos comp, tos  gt  set?, tos w ext, tos ext, next
code  <=        [sp]+ tos comp, tos  gt= set?, tos w ext, tos ext, next
code u>         [sp]+ tos comp, tos ult  set?, tos w ext, tos ext, next
code u>=        [sp]+ tos comp, tos ult= set?, tos w ext, tos ext, next
code u<         [sp]+ tos comp, tos ugt  set?, tos w ext, tos ext, next
code u<=        [sp]+ tos comp, tos ugt= set?, tos w ext, tos ext, next

code d0=        [sp]+ tos or, tos z=  set?, tos w ext, tos ext, next
code d0<>       [sp]+ tos or, tos z<> set?, tos w ext, tos ext, next

code under+     tos [sp cell +] add, tos pop, next
code under-     tos [sp cell +] sub, tos pop, next

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
code 0branch    d1 h read, tos test, z= if d1 tp h add, endif tos pop, next
code ?branch    d1 h read, tos test, z= if d1 tp h add, tos pop, endif next

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
code (of)       d1 h read, d2 pop, d2 tos comp,
                z<> if d1 tp h add, d2 tos move, next, endif tos pop, next
code (of?)      d1 h read, tos test, z= if d1 tp h add, tos pop, next, endif 
                4 # sp add, tos pop, next

{ : case        comp-only 0 ; }
{ : of          comp-only (of)  (orig) ; }
{ : of?         comp-only (of?) (orig) ; }
{ : endof       comp-only >r >r branch (orig) r> 1+ r> (orig>) ; }
{ : endcase     comp-only } drop { 0 ?do (orig>) loop ; }

( ---------------------------------------------------------------------------- )
code   (do)     [sp]+ -[rp] move, tos rpush, tos pop, next
code  (?do)     d1 h read, [sp] tos comp, ^ (do) displacement z<> branch?,
                d1 tp h add, 4 # sp add, tos pop, next
code  (loop)    d1 h read, d2 rpop, d2 inc, [rp] d2 comp,
                z<> if d1 tp h add, d2 rpush, next, endif 4 # rp add, next
code (+loop)    d1 h read, d2 rpop, d3 rpeek, d3 d2 comp, d4 neg set?,
                tos d2 add, d3 d2 comp, tos neg set?, tos d4 b comp,
                z= if d1 tp h add, d2 rpush, tos pop, next, endif
                cell # rp add, tos pop, next
code (-loop)    d1 h read, d2 rpop, d3 rpeek, d2 d3 comp, d4 neg set?,
                tos d2 sub, d2 d3 comp, tos neg set?, tos d4 b comp,
                z= if d1 tp h add, d2 rpush, tos pop, next, endif
                cell # rp add, tos pop, next
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
(       Exceptions                                                             )
( ---------------------------------------------------------------------------- )
alignram 8 cells allot hvariable (estack)

: init-exceptions ( -- ) [ (estack) $FFFF and ] literal (estack) h! ;

code (epush) ( -- )
    (estack) [#] a3 lea, -1 # d1 move, [a3] d1 h move, d1 a1 move,
    rp -[a1] h move, sp -[a1] h move, a1 [a3] h move, next
code (epop) ( -- )
    (estack) [#] a3 lea, -1 # d1 move, [a3] d1 h move, d1 a1 move,
    [a1]+ d1 h move, d1 sp move, [a1]+ d1 h move, d1 rp move,
    a1 [a3] h move, next
code (edrop) ( -- ) (estack) [#] a3 lea, cell # [a3] h add, next

: catch ( xt -- ) (epush) execute (edrop) 0 ;
: throw  ( n -- ) ?if (epop) endif ;

rawcode (throw) \ TOS=throw code
    (estack) [#] a3 lea, -1 # d1 move, [a3] d1 h move, d1 a1 move,
    [a1]+ d1 h move, d1 sp move, [a1]+ d1 h move, d1 rp move,
    a1 [a3] h move, tp rpop, next

Assembler68k definitions  { : throw-primitive, (throw) displacement branch, ; }
Forth definitions

( ---------------------------------------------------------------------------- )
(       Other Words                                                            )
( ---------------------------------------------------------------------------- )
code sp@    tos push, sp tos move, next
code sp!    tos sp move, tos pop,  next
code rp@    tos push, rp tos move, next
code rp!    tos rp move, tos pop,  next
code tp@    tos push, tp tos move, next
code tp!    tos tp move, tos pop,  next
code np@    tos push, np tos move, next
code np!    tos np move, tos pop,  next

alignram StackSize buffer: sp-limit&
         StackSize buffer: sp&      aka rp-limit&         0 buffer: rp&

FloatStack [IF]
    aka fp-limit&    StackSize allot 0 buffer: fp&
    code fdepth tos push, fp& # tos move, fp tos sub, 2 # tos asr, next
    code fp@    tos push, fp tos move, next
    code fp!    tos fp move, tos pop,  next
[THEN]

code rdepth tos push, rp& # tos move, rp tos sub, 2 # tos asr, next
code  depth sp d1 move, tos push, sp& # tos move, d1 tos sub, 2 # tos asr, next

code   mux  tos d1 move, [sp]+ tos and, d1 not, [sp]+ d1 and, d1 tos or, next
code demux  tos d1 move, [sp]  tos and, d1 not, d1 [sp]  and, next

( ---------------------------------------------------------------------------- )
code bounds d1 pop, tos [sp] add, d1 tos move, next

( ---------------------------------------------------------------------------- )
code ms ( n -- )
    tos test, gt if tos begin 708 d1 do loop loop endif tos pop, next

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
















