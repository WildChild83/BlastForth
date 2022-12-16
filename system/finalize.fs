( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Stuff to be done right before the ROM file is written to disk          )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Host definitions

Verbose [IF] cr .( Finalizing... ) [THEN]

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       allocate.fs                                                            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
ramspace @ } ' pad >body { romh!      \ set value of "pad"

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       System Start-Up Routine                                                )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
(       Assembly Part                                                          )
( ---------------------------------------------------------------------------- )
68k-start: asm \ CPU vector table points here

    \ disable CPU interrupts
    $2700 # sr move,

    \ Trademark Security System check
    $A10000 [#] a1 address,
    [a1 $8 +] test, z= if
        [a1 $C +] w test, z= if
            [a1 $1 +] d1 b move, $F # d1 b and, z<> if
                $53454741 # [a1 $4000 +] move,
    endif endif endif

    \ clear System RAM and CPU registers
    rp clear, $deadce11 # d1 move, 64 kilobytes cell/ d2 do d1 rpush, loop
    [rp]+ [[ d1 d2 d3 d4 d5 d6 d7 tos a1 a2 a3 $200C tp sp rp np ]] movem,

    \ initialize stack pointers
    (rp-empty) [#] rp address, (sp-empty) [#] sp address,
               FloatStack [IF] (fp-empty) [#] fp address, [THEN]

    \ launch threading mechanism, transition to Forth code execution
    next& [#] np address,  $4BFA0004 , next ]

( ---------------------------------------------------------------------------- )
(       Forth part                                                             )
( ---------------------------------------------------------------------------- )
    DEBUG [IF] ['] <exit> is exit [THEN]    \ "exit" is vectored in debug mode

    \ sound driver
    init-audio

    \ video hardware
    begin dma? not until -video
    clear-vram   (init-video)  default-video-config  init-dma  init-graphics
    ['] noop dup  is (scrollX)  is (scrollY)
    
    \ Forth environment
    init-exceptions   0 to #frames   decimal
    init-memory       zero-controllers

    \ terminal text display
    15 to text-color-index       $0000 load-glyph-data   ['] <emit> is emit
     0 to attributes    foreground-table terminal page   ['] <type> is type
     +video

    \ call user's entry point, catching any thrown exceptions
    ['] entry catch

    \ error if execution returns from entry point
    DEBUG [IF] dup 0= if drop -265 endif [THEN] crash-system ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )














