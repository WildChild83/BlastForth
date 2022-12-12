( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Controller Input                                                       )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - 68k.fs                                                           )
(           - forth.fs                                                         )
(           - core.fs                                                          )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
(       Ports                                                                  )
( ---------------------------------------------------------------------------- )
$A10003 constant controller-data1       $A10009 constant controller-ctrl1
$A10005 constant controller-data2       $A1000B constant controller-ctrl2

( ---------------------------------------------------------------------------- )
(       Controller States                                                      )
( ---------------------------------------------------------------------------- )
value controller1           value old-controller1
value controller2           value old-controller2
value controller3           value old-controller3
value controller4           value old-controller4

: zero-controllers ( -- ) controller1 [ 8 cells ] literal erase ;

( ---------------------------------------------------------------------------- )
(       Button Presses                                                         )
( ---------------------------------------------------------------------------- )
{:} button: ( c -- ) create c,   ;code ( controller -- flag )
    [dfa] d1 c move, d1 tos test-bit, tos z= set?, tos c extend, next

0 button: up?           4 button: b?            16 button: z?
1 button: down?         5 button: c?            17 button: y?
2 button: left?         6 button: a?            18 button: x?
3 button: right?        7 button: start?        19 button: mode?

( ---------------------------------------------------------------------------- )
(       Controller Read Routines                                               )
( ---------------------------------------------------------------------------- )
defer read-controllers

code <2pad3@> ( -- )
    controller-data1 [#] a1 address,  controller-data2 [#] a2 address,
    d1 clear, d2 clear, $00 # d3 move, $40 # d4 move,
    d4 [a1 6 +] c move, d4 [a2 6 +] c move, d3 [a1] c move, d3 [a2] c move,
    [a1] d1 c move, $30 # d1 c and, 2 # d1 c lsl, d4 [a1] c move, 
    [a2] d2 c move, $30 # d2 c and, 2 # d2 c lsl, d4 [a2] c move,    
    [a1] d5 c move, $3F # d5 c and, d5 d1 c or,
    [a2] d5 c move, $3F # d5 c and, d5 d2 c or,
    controller1 [#] a3 address, d1 [a3]+ move, d2 [a3]+ move, next

code <2pad6@> ( -- )
    controller-data1 [#] a1 address,  controller-data2 [#] a2 address,
    d1 clear, d2 clear, $00 # d3 move, $40 # d4 move,
    d4 [a1 6 +] c move, d4 [a2 6 +] c move, d3 [a1] c move, d3 [a2] c move,
    d4 [a1] c move, d4 [a1] c move, d3 [a1] c move, d3 [a1] c move,
    [a1] d1 c move, $30 # d1 c and, 2 # d1 c lsl, d4 [a1] c move,
    [a2] d2 c move, $30 # d2 c and, 2 # d2 c lsl, d4 [a2] c move,
    [a1] d5 c move, $3F # d5 c and, d5 d1 c or,
    [a2] d5 c move, $3F # d5 c and, d5 d2 c or,
    d3 [a1] c move, d3 [a2] c move, d4 [a1] c move, d4 [a2] c move,
    d1 swap, [a1] d1 c move, d1 swap, d2 swap, [a2] d2 c move, d2 swap,
    controller1 [#] a3 address, d1 [a3]+ move, d2 [a3]+ move, next

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )









