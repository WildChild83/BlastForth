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
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
: (system-crash) ( sp  rp  throw-code -- )
    DEBUG [IF] ['] <exit> is exit [THEN]
    begin vdp-dma? not until
    2autoinc $0000 write-vram 64 kilobytes cell/ 0 do 0 >vdp loop
    (init-video-config)
    2autoinc $C000 planeA $E000 planeB +h320 +v224 +ntsc
    64x64planes 255 hbi-counter !video
    $00 write-cram $0000 h>vdp $0EEE h>vdp
    1 to text-color-index    $0000 load-glyph-data     ['] <emit> is emit
    0 to attributes          planeA> terminal page     +video

    cr cr ." Error Code: " .

    cr cr ." Return Stack: "
    cr rp& swap 3 cells - ?do i @ hex. cell +loop

    cr cr ." Data Stack: "
    cr sp& swap 3 cells - ?do i @ . cell +loop    
    
    cr cr ." CPU halted."
    cr ." Reset the console to continue."
    begin again ;

( ---------------------------------------------------------------------------- )
code system-crash ( throw-code -- ) PC: system-crash-code
    $2700  #  sr move,
    $8104  # vdp-ctrl [#] h move,
    rp d1 move, rp clear, sp d2 move, [rp -128 +] sp lea,
    0 # -[sp] move, d2 push, d1 push,
    next& [#] np lea,
    ' (system-crash) execute,
     end

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















