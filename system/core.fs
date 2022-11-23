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
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
(       State-Smart Words                                                      )
( ---------------------------------------------------------------------------- )
-1 constant true
 0 constant false
 4 constant cell
 2 constant half

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
{ ' umin }      anon d1 pop, d1 tos comp,
                    ugt if d1 tos move, endif next  host/target: umin
{ ' umax }      anon d1 pop, d1 tos comp,
                    ult if d1 tos move, endif next  host/target: umax
{ '  min }      anon d1 pop, d1 tos comp, 
                     gt if d1 tos move, endif next  host/target: min
{ '  max }      anon d1 pop, d1 tos comp, 
                     lt if d1 tos move, endif next  host/target: max

{ ' 0= }        anon tos test, tos z= set?,
                    tos w ext, tos ext, next        host/target:  0=  aka not
{ ' 0<> }       anon tos test, tos z<> set?,
                    tos w ext, tos ext, next        host/target:  0<> aka flag

{ ' pick }      anon 2 # tos lsl,
                    [sp tos+ 0] tos move, next      host/target: pick
{ ' roll }
    anon tos test, z= if tos pop, next, endif
        tos d1 move, 2 # tos lsl, [sp tos+ 4 +] a1 lea, [a1 -4 +] a2 lea,
        [a2] tos move, d6 begin -[a2] -[a1] move, loop 4 # sp add, next
                                                    host/target: roll

{ :noname ; }   anon next   host/target: noop       aka chars

( ---------------------------------------------------------------------------- )
(       Dumb Words  [aka "compile-only"]                                       )
( ---------------------------------------------------------------------------- )
code   @        tos a1 move, [a1] tos move, next
code  h@        tos a1 move, tos clear, [a1] tos h move, next
code  c@        tos a1 move, tos clear, [a1] tos c move, next
code sh@        tos a1 move, [a1] tos h move, tos ext, next
code sc@        tos a1 move, [a1] tos c move, tos h ext, tos ext, next
code   !        tos a1 move, [a1] pop, tos pop, next
code  h!        tos a1 move, 2 # a1 add, [a1] h pop, tos pop, next
code  c!        tos a1 move, 3 # a1 add, [a1] c pop, tos pop, next
code  2@        tos a1 move, [a1]+ tos move, [a1] push, next
code  2!        tos a1 move, [a1]+ pop, [a1]+ pop, tos pop, next
code  +!        tos a1 move, d1 pop, d1 [a1]   add, tos pop, next
code h+!        tos a1 move, d1 pop, d1 [a1] h add, tos pop, next
code c+!        tos a1 move, d1 pop, d1 [a1] c add, tos pop, next
code 2+!        tos a1 move, d1 pop, d2 pop, [a1] d3 move,
                d2 [a1 cell +] add, d1 d3 addx, d3 [a1] move, tos pop, next

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

( ---------------------------------------------------------------------------- )
(       Flow Control Words                                                     )
( ---------------------------------------------------------------------------- )
{ : (disp?) ( n -- )   ?shalf not abort" Destination out of range." ; }
{ : (disp)  ( u -- n ) romspace      - 2 - (disp?) ; }
{ : (disp>) ( u -- n ) romspace swap - 2 - (disp?) ; }

code  branch    [tp]+ tp h add, next
code 0branch    d1 h read, tos test, z= if d1 tp h add, endif tos pop, next

{ : if    ( -- orig ) comp-only 0branch  romspace 0 h, ; }
{ : ahead ( -- orig ) comp-only  branch  romspace 0 h, ; }
{ : then  ( orig -- ) comp-only dup (disp>) swap romh! ; }      aka endif

{ : begin ( -- dest ) comp-only romspace ; }
{ : again ( dest -- ) comp-only  branch (disp) h, ; }
{ : until ( dest -- ) comp-only 0branch (disp) h, ; }

{ : else  ( orig1 -- orig2 )     comp-only } ahead { swap } then { ; }
{ : while  ( dest -- orig dest ) comp-only } if { swap ; }
{ : repeat ( orig dest -- )      comp-only } again then { ; }

( ---------------------------------------------------------------------------- )
(       Other Words                                                            )
( ---------------------------------------------------------------------------- )
code sp@    tos push, sp tos move, next
code sp!    tos sp move, tos pop,  next
code rp@    tos push, rp tos move, next
code rp!    tos rp move, tos pop,  next
code np@    tos push, np tos move, next
code np!    tos np move, tos pop,  next

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
















