( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Uncaught Exception Handling                                            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - 68k.fs                                                           )
(           - forth.fs                                                         )
(           - core.fs                                                          )
(           - text.fs                                                          )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

DEBUG [IF]

( ---------------------------------------------------------------------------- )
create (exception-names)
    cstring" Unknown Error"                     \   0
    cstring" Abort "                            \  -1
           1 zeroes,
    cstring" Data Stack Overflow"               \  -3
    cstring" Data Stack Underflow"              \  -4
    cstring" Return Stack Overflow"             \  -5
    cstring" Return Stack Underflow"            \  -6
           2 zeroes,
    cstring" Invalid Memory Address"            \  -9
    cstring" Division by Zero"                  \ -10
    cstring" Result out of Range"               \ -11
    cstring" Argument Type Mismatch"            \ -12
           4 zeroes,
    cstring" Numeric Output String Overflow"    \ -17
    cstring" Parsed String Overflow"            \ -18
           1 zeroes,
    cstring" Write to Read-Only Location"       \ -20
    cstring" Unsupported Operation"             \ -21
           1 zeroes,
    cstring" Address Alignment Exception"       \ -23
    cstring" Invalid Numeric Argument"          \ -24
    cstring" Return Stack Imbalance"            \ -25
           7 zeroes,
    cstring" Block Read Exception"              \ -33
    cstring" Block Write Exception"             \ -34
    cstring" Invalid Block Number"              \ -35
           4 zeroes,
    cstring" Invalid Floating-Point Base"       \ -40
    cstring" Loss of Precision"                 \ -41
    cstring" Floating-Point Divide by Zero"     \ -42
    cstring" Floating-Point Out of Range"       \ -43
    cstring" Float Stack Overflow"              \ -44
    cstring" Float Stack Underflow"             \ -45
    cstring" Invalid Floating-Point Argument"   \ -46
           6 zeroes,
    cstring" Exception Stack Overflow"          \ -53
    cstring" Floating-Point Underflow"          \ -54
    cstring" Floating-Point Unidentified Fault" \ -55
           3 zeroes,
    cstring" Memory Allocate Exception"         \ -59
    cstring" Memory Free Exception"             \ -60
    cstring" Memory Resize Exception"           \ -61
          16 zeroes,
    cstring" String Substitute Exception"       \ -78
    cstring" String Replaces Exception"         \ -79

( ---------------------------------------------------------------------------- )
create (blastforth-exceptions)
    cstring" CPU Bus Error!"                    \ -256
    cstring" CPU Illegal Instruction"           \ -257
    cstring" CPU Check Exception"               \ -258
    cstring" CPU Trap Vector Exception"         \ -259
    cstring" CPU Privilege Violation"           \ -260
    cstring" CPU Trace Exception"               \ -261
    cstring" Uninitialized CPU Interrupt"       \ -262
    cstring" CPU Spurious Interrupt"            \ -263
    cstring" CPU Trap Exception"                \ -264

    cstring" End of User Code"                  \ -265
    cstring" DMA Queue Overflow"                \ -266

( ---------------------------------------------------------------------------- )
value (abort-string)

: exception>string ( n -- addr u )
    dup -2 = if drop (abort-string) count exit endif
    0 min negate dup 256 267 within
    if 256 - (blastforth-exceptions) else dup 79 <= and (exception-names) endif
    swap 0 ?do count + loop count ?if exit endif drop (exception-names) count ;

( ---------------------------------------------------------------------------- )
code (<exit>) tp rpull, next

: (crash-system) ( sp  rp  throw-code -- )
    ['] (<exit>) is exit
    begin dma? not until
    2 autoinc $0000 write-vram 64 kilobytes cell/ 0 do 0 !video loop
    (init-video-config)
    2 autoinc $C000 planeA $E000 planeB +h320 +v224
    64x64planes 255 hbi-counter set-video
    $00 write-cram $0000 h!video $0EEE h!video
    1 to text-color-index    $0000 load-glyph-data     ['] <emit> is emit
    0 to attributes          planeA> terminal page     ['] <type-words> is type
    +video
    
    cr cr dup exception>string type    
    cr cr ." Throw Code:" .

    (eptrs)
    cr cr ." Return Stack:"
    cr (rp-empty) tuck min    
        2dup = if 2drop ." (empty)" else do i @ hex. cell +loop endif
    
    cr cr ." Data Stack:"
    cr (sp-empty) tuck min
        2dup = if 2drop ." (empty)" else do i @ . cell +loop endif
    
    FloatStack [IF]
        cr cr ." Float Stack: "
        cr (fp-empty) fp@ over min
            2dup = if 2drop ." (empty)" else do i @ hex. cell +loop endif
    [THEN]
    
    cr cr ." Reset the console to continue."
    begin again ;

( ---------------------------------------------------------------------------- )
code crash-system ( throwcode -- )
    $2700 # sr move, $8104 # video-ctrl [#] h move,
    $FFFFF0 [#] rp lea, [rp -128 +] sp lea, next& [#] np lea,
    ' (crash-system) execute, end

( ---------------------------------------------------------------------------- )
code <exit>
    rp d1 move, (rp-empty) $FFFF and # d1 h compare,
        ugt= if tos push, -6 # tos move, ' crash-system >body primitive, endif
    sp d1 move, (sp-empty) $FFFF and # d1 h compare,
         ugt if -4 # tos move, ' crash-system >body primitive, endif
    (sp-limit) $FFFF and # d1 h compare,
         ult if -3 # tos move, ' crash-system >body primitive, endif
    FloatStack [IF]
        fp d1 move, (fp-empty) $FFFF and # d1 h compare,
            ugt= if -45 # tos move, ' crash-system >body primitive, endif
        (fp-limit) $FFFF and # d1 h compare,
            ult= if -44 # tos move, ' crash-system >body primitive, endif
    [THEN]
    tp rpull, next

( ---------------------------------------------------------------------------- )
[ELSE]  \ if DEBUG=false

code crash-system ( -- ) 4 [#] a1 move, [a1] jump, end \ perform a soft reset

( ---------------------------------------------------------------------------- )
[THEN]

( ---------------------------------------------------------------------------- )
(       Hardware Exceptions                                                    )
( ---------------------------------------------------------------------------- )
68k-bus:        asm -256 # tos move, throw-primitive, end
68k-address:    asm  -23 # tos move, throw-primitive, end
68k-illegal:    asm -257 # tos move, throw-primitive, end
68k-divzero:    asm  -10 # tos move, throw-primitive, end
68k-check:      asm -258 # tos move, throw-primitive, end
68k-trapv:      asm -259 # tos move, throw-primitive, end
68k-privilege:  asm -260 # tos move, throw-primitive, end
68k-trace:      asm -261 # tos move, throw-primitive, end
68k-uninit:     asm -262 # tos move, throw-primitive, end
68k-spurious:   asm -263 # tos move, throw-primitive, end
68k-alltraps:   asm -264 # tos move, throw-primitive, end

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















