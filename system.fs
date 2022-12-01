( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Blast Forth's Root File                                                )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
include system/glossary.fs

( ---------------------------------------------------------------------------- )
(       Project Attributes                                                     )
( ---------------------------------------------------------------------------- )
Forth definitions {

\ Rom output file size.  Max supported size is 4 megabytes.
\   This number can also be specified in "kilobytes" or "bytes".
1 megabytes make SizeOfROM

\ If DEBUG=true then program includes extra checks and error reporting
\   (and larger, slower code).  Set to false for "release version."
true make DEBUG

\ Verbose directs the compiler to print detailed information to the terminal.
true make Verbose

\ Perform various compile-time optimizations for faster execution.
true make Optimization

\ If FloatStack=true, the program will be built with floating-point support,
\   including a dedicated stack for floating-point numbers.
false make FloatStack

\ The Data Stack, Return Stack, and Float Stack (if used) will each occupy
\   "StackSize" bytes in RAM.  Must be a multiple of 4.
128 bytes make StackSize

\ Forth environments traditionally provide "floored" division, but the 68k CPU
\   only performs "symmetric" division.  If you don't know the difference then
\   just leave this attribute false.  This post explains the (gory) details:
\       www.nimblemachines.com/symmetric-division-considered-harmful/
false make FlooredDivision

( ---------------------------------------------------------------------------- )
include system/romfile.fs

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Sega ROM Header                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Forth definitions

( Console )      ascii" SEGA GENESIS    "
( Copyright )    ascii" (C)NAME 20xx.JAN"
( Domestic )     ascii" Japanese Name                                   "
( Foreign )      ascii" Western Name                                    "
( Version # )    ascii" GM 00000001-01"
( Checksum )            0 h,
( I/O Support )  ascii" J               "
( ROM Start )           0 ,
( ROM End )     SizeOfROM { 1- } ,
( RAM Start )     $FF0000 ,
( RAM End )       $FFFFFF ,
( SRAM? )               0 ,
( -- )                  0 ,
( SRAM Start )          0 ,
( SRAM End )            0 ,
( -- )              0 , 0 ,
( Notes )        ascii"                                         "
( Countries )    ascii" JUE             "

( ---------------------------------------------------------------------------- )
\ Warn if header is the wrong size (comment this line to disable the warning)
romspace { 512 bytes <> } [IF] .( Incorrect Sega ROM Header. ) [THEN]

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Code Base                                                              )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
include system/68k.fs       \ Motorola 68000 assembler
include system/forth.fs     \ Forth cross-compiler, standard defining words
include system/core.fs      \ all the most important Forth words

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       The "Next" Mechanism   [the heart of the whole system]                 )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Forth definitions

\ fetch the next cell from the instruction thread and jump to it
create next&  asm
    dfa read,           \ DFA = code field address [from instruction thread]
    [dfa]+ a1 move,     \  A1 = code field,  DFA = data field address
    [a1] jump,          \ jump to code field
    end

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Floating-Point Stack                                                   )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Forth definitions

FloatStack [IF]     \ If using floats, uncomment one and ONLY one of these:
    include system/floatstack/float16.fs   \ 17-bit mantissa, 16-bit exponent
    \ include system/floatstack/float32.fs   \ 33-bit mantissa, 32-bit exponent
    \ include system/floatstack/ieee32.fs    \ 24-bit mantissa,  8-bit exponent
    \ include system/floatstack/ieee64.fs    \ 48-bit mantissa, 16-bit exponent
[THEN]

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Sound Driver                                                           )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
include system/audio/z80.fs

( ---------------------------------------------------------------------------- )
include system/audio/silence.fs

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Other Hardware                                                         )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
include system/vdp.fs           \ video display processor
include system/text.fs          \ text display, terminal output
include system/interrupts.fs    \ vertical and horizontal blank handlers

DEBUG [IF] : <exit> rdrop r> tp! ; [THEN]

include system/exceptions.fs    \ default exception handlers

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Default Video Configuration                                            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Forth definitions

\ system start-up will place the video hardware into the following state:
: init-video ( -- )
    (init-video-config)       2autoinc 
    $C000 planeA        $B800 spritePlane       +h320 +v224 +ntsc
    $E000 planeB        $BC00 hScrollTable      64x64planes
    $B000 windowPlane     255 hbi-counter       !video ;

\ default palette will be copied into Palette 0 of Color RAM
create default-palette
    $0000 h, $0008 h, $0080 h, $0800 h, $0088 h, $0808 h, $0880 h, $0AAA h,
    $0444 h, $000E h, $00E0 h, $0E00 h, $00EE h, $0E0E h, $0EE0 h, $0EEE h,

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Program Entry Point                                                    )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
(       User Code entry point                                                  )
( ---------------------------------------------------------------------------- )
create (:entry) 0 ,
{ : :entry   host-only  PC (:entry) rom!  docolon& , } ] { ; }

( ---------------------------------------------------------------------------- )
(       System Start-Up: Assembly part                                         )
( ---------------------------------------------------------------------------- )
68k-start: asm

    \ disable CPU interrupts
    $2700 # sr move,
    
    \ Trademark Security System check
    $A10000 [#] a1 lea,
    [a1 $8 +] test, z= if
        [a1 $C +] w test, z= if
            [a1 $1 +] d1 b move, $F # d1 b and, z<> if
                $53454741 # [a1 $4000 +] move,
    endif endif endif

    \ clear System RAM and CPU registers
    rp clear, $deadce11 # d1 move, 64 kilobytes cell/ d2 do d1 rpush, loop
    [rp]+ [[ d1 d2 d3 d4 d5 d6 d7 tos a1 a2 a3 fp tp sp rp np ]] movem,

    \ initialize stack pointers
    rp& [#] rp lea, sp& [#] sp lea, FloatStack [IF] fp& [#] fp lea, [THEN]

    \ launch threading mechanism, transition to Forth code execution
    next& [#] np lea,  $4BFA0004 , next ]

( ---------------------------------------------------------------------------- )
(       System Start-Up: Forth part                                            )
( ---------------------------------------------------------------------------- )
    DEBUG [IF] ['] <exit> is exit [THEN]    \ "exit" is vectored in DEBUG mode

    \ sound driver
    \ ...

    \ video hardware
    begin vdp-dma? not until init-video
    default-palette $00 16 store-color
    
    \ Forth environment
    init-exceptions   0 to #vblanks   decimal

    \ terminal text display
    15 to text-color-index    $0000 load-glyph-data     ['] <emit> is emit
     0 to attributes          planeA> terminal page     ['] <type> is type
     +video

    \ call user's entry point, catching any thrown exceptions
    (:entry) @ catch
    
    \ display error code and stop
    dup 0= if drop 1 endif system-crash ;
    
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Hardware Cheat Sheets                                                  )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
\   68k Status Register     15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0
\                           T  -  S  -  -  I2 I1 I0  -  -  -  X  N  C  V  Z
\       T:  Trace Mode
\       S:  Supervisor State
\       I:  Interrupt Priority Mask (VBI=6, HBI=4, EXT=2)
\       X:  Extend Bit      \
\       N:  Negative Bit     \
\       C:  Carry Bit         > Condition Codes
\       V:  Overflow Bit     /
\       Z:  Zero Bit        /
\                                                                              
( ---------------------------------------------------------------------------- )
\   VDP Status Register     15 14 13 12 11 10  9  8  7  6  5  4  3  2  1   0
\                            -  -  -  -  -  -  E  F VI SO SC OD VB HB DMA PAL
\       E:   FIFO is empty
\       F:   FIFO is full
\       VI:  Vertical Interrupt occurred
\       SO:  Sprite scanline limit reached (17+ for 256-wide, 21+ for 320)
\       SC:  Sprite collision
\       OD:  Odd frame displayed (interlace mode only)
\       VB:  Vertical blank in progress
\       HB:  Horizontal blank in progress
\       DMA: DMA in progress
\       PAL: This is a PAL console (0=NTSC)
\ 
( ---------------------------------------------------------------------------- )
\   Z80 Flags Register                         7   6   5   4   3   2   1   0
\                                              S   Z   -   H   -  P/V  N   C
\       S:   Sign
\       Z:   Zero
\       H:   Half Carry
\       P/V: Parity/Overflow
\       N:   Add/Subtract
\       C:   Carry
\ 
( ---------------------------------------------------------------------------- )
\   68k Memory Map (24 bit)
\       $000000     ROM Cartridge
\       $100000      |
\       $200000      |
\       $300000      V
\       $400000     [SEGA Reserved]
\       $500000      |   
\       $600000      |
\       $700000      |
\       $800000      |
\       $900000      V
\       $A00000     System I/O
\       $B00000     [SEGA Reserved]
\       $C00000     Visual Display Processor (VDP)
\       $D00000      V
\       $E00000     [Access Prohibited]
\       $F00000     RAM Area ($FF0000-$FFFFFF) (64kB)
\       $FFFFFF     [END]
\ 
\       System I/O
\           $A00000     Z80
\           $A10000     I/O
\           $A11000     Control
\           $A12000     [SEGA Reserved]
\           $AFFFFF     [END]
\ 
\           Z80 Area
\               $A00000     Z80 RAM (8kB)
\               $A02000     [SEGA Reserved]
\               $A04000     YM2612
\               $A06000     Bank Register, PSG
\               $A08000     68k RAM Window (32kB)
\               $A0A000      |
\               $A0C000      |
\               $A0E000      V
\               $A0FFFF     [END]
\ 
\           I/O Area
\               $A10000     Version Number
\               $A10002     Data (CTRL 1, CTRL 2, EXP)
\               $A10008     Control (1, 2, E)
\               $A1000E     TxDATA, RxData, S-Mode (1)
\               $A10014     TxDATA, RxData, S-Mode (2)
\               $A1001A     TxDATA, RxData, S-Mode (3)
\               $A10020     [Access Prohibited]
\               $A10FFF     [END]
\ 
\           Control Area
\               $A11000     Memory Mode
\               $A11002     [Access Prohibited]
\               $A11100     Z80 Bus Request
\               $A11102     [Access Prohibited]
\               $A11200     Z80 Reset
\               $A11202     [Access Prohibited]
\               $A11FFF     [END]
\ 
\       VDP Area
\           $C00000     Data
\           $C00004     Control
\           $C00008     HV Counter
\           $C0000A     [Access Prohibited]
\           $C00011     PSG
\           $C00012     [Access Prohibited]
\           $DFFFFF     [END]
\ 
\       RAM Area
\           $F00000     [Access Prohibited]
\           $FF0000     Work RAM (64kB)
\           $FFFFFF     [END]
\ 
( ---------------------------------------------------------------------------- )
\   Z80 Memory Map (16 bit)
\       $0000   Sound RAM (8kB)
\       $2000   [SEGA Reserved]
\       $4000   Sound Chip (YM2612)
\       $6000   Bank Register, PSG
\       $8000   68k RAM Bank Window (32kB)
\       $A000    |
\       $C000    |
\       $E000    V
\       $FFFF   [END]
\ 
\       Sound Chip Area
\           $4000   FM1 Register Select (Channel 1-3)
\           $4001   FM1 Data
\           $4002   FM2 Register Select (Channel 4-6)
\           $4003   FM2 Data
\           $4004   [Access Prohibited]
\           $5FFF   [END]
\ 
\       Bank Register Area
\           $6000   Bank Register
\           $6001   [Access Prohibited]
\           $7F11   PSG 76489
\           $7F12   [Access Prohibited]
\           $7FFF   [END]
\ 
( ---------------------------------------------------------------------------- )
\   Default VRAM Layout (64 kB)
\       $0000   Work VRAM
\       $2000    |
\       $4000    |
\       $6000    |
\       $8000    |
\       $A000    V
\       $B000   Window Nametable ($B000), Sprite Table ($B800), HScroll ($BC00)
\       $C000   Screen A Nametable
\       $D000   
\       $E000   Screen B Nametable
\       $F000   
\ 
( ---------------------------------------------------------------------------- )
\   Nametable Entry (2 bytes)
\      ---------------------------------------------------------------------
\     | 15        14-13    12      11      10-0                             |
\     | Priority  Palette  V-Flip  H-Flip  Tile Index                       |
\      ---------------------------------------------------------------------
\ 
\   Sprite Attribute Table Entry (8 bytes)
\      ---------------------------------------------------------------------
\     | 15-10                                          9-0                  |
\    0| Unused                                         V-Pos                |
\     |---------------------------------------------------------------------|
\     | 15-12                      11-10               9-8     7       6-0  |
\    1| Unused                     H-Size              V-Size  Unused  Link |
\     |---------------------------------------------------------------------|
\     | 15        14-13    12      11      10-0                             |
\    2| Priority  Palette  V-Flip  H-Flip  Tile Index                       |
\     |---------------------------------------------------------------------|
\     | 15-9                                               8-0              |
\    3| Unused                                             H-Pos            |
\      ---------------------------------------------------------------------
\ 
( -------------------------------------------------------------------------    )
\   VDP Registers                           7   6   5   4   3   2   1   0  |
\       $00 Mode Register 1                 0   0   L  IE1  0   1  M3   0  |
\           L:   Blank 8 left-most pixels
\           IE1: Horizontal Interrupt Enable
\           M3:  HV Counter Latch  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $01 Mode Register 2                 0  DE  IE0 M1  M2   1   0   0  |
\           DE:  Display Enable (preferred)
\           IE0: Vertical Interrupt Enable
\           M1:  DMA Enable
\           M2:  0=NTSC (224 pixels), 1=PAL (240 pixels)
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $02 Screen A Nametable Address      0   0  A15 A14 A13  0   0   0  |
\ 
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $03 Window Nametable Address        0   0  W15 W14 W13 W12 W11  0  |
\           *W11 is ignored in 320 wide mode
\ 
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $04 Screen B Nametable Address      0   0   0   0   0  B15 B14 B13 |
\ 
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $05 Sprite Attr. Table Address      0  T15 T14 T13 T12 T11 T10 T9  |
\           *T9 is ignored in 320 wide mode
\ 
\       $06 Not Used (128kB VRAM only)
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $07 Background Color                0   0  P1  P0  C3  C2  C1  C0  |
\           P:  Palette Index (0-3)
\           C:  Color Index (0-15)
\ 
\       $08 Not Used (Sega Master System only)
\ 
\       $09 Not Used (Sega Master System only)
\ 
\       $0A Horizontal Interrupt Counter (8 bit value)
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $0B Mode Register 3                 0   0   0   0  IE2 VS  HS1 HS2 |
\           IE2: External Interrupt Enable
\           VS:  0=entire screen, 1=strips (16 pixels)
\           HS:  00=entire screen, 01=prohibited, 10=strips (8 px), 11=scanline
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $0C Mode Register 4                RS0  0   0   0  SHI L1  L0  RS1 |
\           RS0,RS1: 0=256-wide mode, 1=320-wide mode
\           SHI: Shadow/Highlight Enable
\           L:   00=interlace off, 01=on, 10=prohibited, 11=double resolution
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $0D H-Scroll Table Address          0   0  H15 H14 H13 H12 H11 H10 |
\ 
\       $0E Not Used (128kB VRAM only)
\ 
\       $0F VRAM Auto-Increment (8 bit value)
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $10 Screen A and B Size             0   0  V1  V0   0   0  H1  H0  |
\           V=Vertical, H=Horizontal
\           00=32 tiles, 01=64 tiles, 10=prohibited, 11=128 tiles
\           (64x128, 128x64, and 128x128 are not allowed)
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $11 Window Horizontal Position      R   0   0  H5  H4  H3  H2  H1  |
\           R:  0=left, 1=right
\           H:  position in 2-cell increments
\                                  ----------------------------------------
\                                           7   6   5   4   3   2   1   0  |
\       $12 Window Vertical Position        D   0   0  V4  V3  V2  V1  V0  |
\           D:  0=up, 1=down
\           V:  position in 1-cell increments
\ 
\       $13-$14 DMA Length
\ 
\       $15-$17 DMA Source
\ 
( ---------------------------------------------------------------------------- )
\   Vector Table Indices
\       0:  Initial stack pointer value
\       1:  Start of program
\       2:  Bus error
\       3:  Address error
\       4:  Illegal instruction
\       5:  Integer divide by zero
\       6:  CHK exception
\       7:  TRAPV exception
\       8:  Privilege violation
\       9:  TRACE exception
\      10:  Line-A emulator
\      11:  Line-F emulator
\      12:  Unassigned (reserved)
\      13:  Coprocessor Protocol violation
\      14:  Format Error
\      15:  Uninitialized Interrupt
\   16-23:  Unused (reserved)
\      24:  Spurious exception
\      25:  IRQ level 1
\      26:  IRQ level 2 (external interrupt)
\      27:  IRQ level 3
\      28:  IRQ level 4 (horizontal retrace)
\      29:  IRQ level 5
\      30:  IRQ level 6 (vertical retrace)
\      31:  IRQ level 7
\   32-47:  TRAP Exception #0-15
\   48-63:  Unused (reserved)
\ 
( ---------------------------------------------------------------------------- )
\   YM-2612 Registers
\ 
\    **********************
\    ** GLOBAL REGISTERS **
\    **********************
\ 
\       READ DATA                               7   6   5   4   3   2   1   0
\           Busy:   1=chip busy, 0=ready       Busy -   -   -   -   -  V.B V.A
\           V:      1=Timer A/B overflowed, 0=not yet
\ 
\                                               7   6   5   4   3   2   1   0
\       $22: LFO                                -   -   -   -  EN  FREQ******
\           EN:     1=enabled, 0=disabled
\           FREQ:   000 =  3.98 hz
\                   001 =  5.56 hz
\                   010 =  6.02 hz
\                   011 =  6.37 hz
\                   100 =  6.88 hz
\                   101 =  9.63 hz
\                   110 = 48.10 hz
\                   111 = 72.20 hz
\                                               7   6   5   4   3   2   1   0
\       $24: Timer A (MSB's)                   MSB's*************************
\           MSB's:  Bits 10-2 of Timer A value
\                                               7   6   5   4   3   2   1   0
\       $25: Timer A (LSB's)                    -   -   -   -   -   -   LSB's
\           LSB's:  Bits 1-0 of Timer A value
\ 
\               Formula: 18 * (1024-(value)) microseconds
\               One tick = 0.018 ms
\               Max time = 18.4 ms
\                                               7   6   5   4   3   2   1   0
\       $26: Timer B                          Value**************************
\ 
\               Formula: 288 * (256-(value)) microseconds
\               One tick = 0.288 ms
\               Max time = 73.44 ms
\ 
\                                               7   6   5   4   3   2   1   0
\       $27: Mode Register                      -  CH3 R.B R.A E.B E.A L.B L.A
\           CH3:    1=four frequencies, 0=one frequency
\           R.A/B:  RESET:  1=reset timer read flag, 0=no effect
\           E.A/B:  ENABLE: 1=enable timer, will now set overflow read flag
\           L.A/B:  LOAD:   1=start timer, 0=stop timer
\ 
\                                               7   6   5   4   3   2   1   0
\       $28: Key On/Off                        O4  O3  O2  O1   -  CH********
\           O4-O1:  Operator 1-4 enable
\           CH:     000 = Channel 1
\                   001 = Channel 2
\                   010 = Channel 3
\                   100 = Channel 4
\                   101 = Channel 5
\                   110 = Channel 6
\                                               7   6   5   4   3   2   1   0
\       $2A: DAC Data                           Data*************************
\ 
\                                               7   6   5   4   3   2   1   0
\       $2B: DAC Enable                        EN   -   -   -   -   -   -   -
\           EN:     1=Ch6 is PCM, 0=Ch6 is FM
\ 
\    ****************************
\    ** PER-OPERATOR REGISTERS **
\    ****************************
\                                               7   6   5   4   3   2   1   0
\       $30: Detune/Multiple (CH1, OP1)         -   DT*******   MULT*********
\       $31: Detune/Multiple (CH2, OP1)             DT:      000 = no change
\       $32: Detune/Multiple (CH3, OP1)                      001 = x (1+1e)
\                                                            010 = x (1+2e)
\       $34: Detune/Multiple (CH1, OP2)                      011 = x (1+3e)
\       $35: Detune/Multiple (CH2, OP2)                      100 = no change
\       $36: Detune/Multiple (CH3, OP2)                      101 = x (1-1e)
\                                                            110 = x (1-2e)
\       $38: Detune/Multiple (CH1, OP3)                      111 = x (1-3e)
\       $39: Detune/Multiple (CH2, OP3)             MULT:   0000 = x 0.5
\       $3A: Detune/Multiple (CH3, OP3)                     0001 = x 1.0
\                                                           0010 = x 2.0
\       $3C: Detune/Multiple (CH1, OP4)                      ...
\       $3D: Detune/Multiple (CH2, OP4)                     1111 = x 15.0
\       $3E: Detune/Multiple (CH3, OP4)
\                                               7   6   5   4   3   2   1   0
\       $40: Total Level (CH1, OP1)             -  Level*********************
\       $41: Total Level (CH2, OP1)
\       $42: Total Level (CH3, OP1)                 0 = loudest
\                                                 127 = softest
\       $44: Total Level (CH1, OP2)
\       $45: Total Level (CH2, OP2)
\       $46: Total Level (CH3, OP2)
\ 
\       $48: Total Level (CH1, OP3)
\       $49: Total Level (CH2, OP3)
\       $4A: Total Level (CH3, OP3)
\ 
\       $4C: Total Level (CH1, OP4)
\       $4D: Total Level (CH2, OP4)
\       $4E: Total Level (CH3, OP4)
\                                               7   6   5   4   3   2   1   0
\       $50: Attack Rate/Rate Scale(CH1, OP1)  RS****   -  Attack************
\       $51: Attack Rate/Rate Scale(CH2, OP1)
\       $52: Attack Rate/Rate Scale(CH3, OP1)
\ 
\       $54: Attack Rate/Rate Scale(CH1, OP2)
\       $55: Attack Rate/Rate Scale(CH2, OP2)
\       $56: Attack Rate/Rate Scale(CH3, OP2)
\ 
\       $58: Attack Rate/Rate Scale(CH1, OP3)
\       $59: Attack Rate/Rate Scale(CH2, OP3)
\       $5A: Attack Rate/Rate Scale(CH3, OP3)
\ 
\       $5C: Attack Rate/Rate Scale(CH1, OP4)
\       $5D: Attack Rate/Rate Scale(CH2, OP4)
\       $5E: Attack Rate/Rate Scale(CH3, OP4)
\                                               7   6   5   4   3   2   1   0
\       $60: Decay Rate/AM Enable (CH1,OP1)    AM   -   -  Decay*************
\       $61: Decay Rate/AM Enable (CH2,OP1)
\       $62: Decay Rate/AM Enable (CH3,OP1)         AM = Amplitude Modulation
\                                                         (via LFO)
\       $64: Decay Rate/AM Enable (CH1,OP2)
\       $65: Decay Rate/AM Enable (CH2,OP2)
\       $66: Decay Rate/AM Enable (CH3,OP2)
\ 
\       $68: Decay Rate/AM Enable (CH1,OP3)
\       $69: Decay Rate/AM Enable (CH2,OP3)
\       $6A: Decay Rate/AM Enable (CH3,OP3)
\ 
\       $6C: Decay Rate/AM Enable (CH1,OP4)
\       $6D: Decay Rate/AM Enable (CH2,OP4)
\       $6E: Decay Rate/AM Enable (CH3,OP4)
\                                               7   6   5   4   3   2   1   0
\       $70: Sustain Rate (CH1, OP1)            -   -   -  Sustain***********
\       $71: Sustain Rate (CH2, OP1)
\       $72: Sustain Rate (CH3, OP1)
\ 
\       $74: Sustain Rate (CH1, OP2)
\       $75: Sustain Rate (CH2, OP2)
\       $76: Sustain Rate (CH3, OP2)
\ 
\       $78: Sustain Rate (CH1, OP3)
\       $79: Sustain Rate (CH2, OP3)
\       $7A: Sustain Rate (CH3, OP3)
\ 
\       $7C: Sustain Rate (CH1, OP4)
\       $7D: Sustain Rate (CH2, OP4)
\       $7E: Sustain Rate (CH3, OP4)
\                                                 7   6   5   4   3   2   1   0
\       $80: Release Rate/Sustain Level(CH1,OP1) Level*********  Release*******
\       $81: Release Rate/Sustain Level(CH2,OP1)
\       $82: Release Rate/Sustain Level(CH3,OP1)    Multiply Level by 8 to 
\                                                    compare it to Total Level.
\       $84: Release Rate/Sustain Level(CH1,OP2)     This register contains the
\       $85: Release Rate/Sustain Level(CH2,OP2)     4 MSBs of a 7-bit number.
\       $86: Release Rate/Sustain Level(CH3,OP2)
\ 
\       $88: Release Rate/Sustain Level(CH1,OP3)    Multiply Release by 2 and 
\       $89: Release Rate/Sustain Level(CH2,OP3)     add 1 to compare it to the
\       $8A: Release Rate/Sustain Level(CH3,OP3)     other rates. This is the
\                                                    4 MSBs of a 5-bit number.
\       $8D: Release Rate/Sustain Level(CH1,OP4)
\       $8C: Release Rate/Sustain Level(CH2,OP4)
\       $8E: Release Rate/Sustain Level(CH3,OP4)
\ 
\                                               7   6   5   4   3   2   1   0
\       $90: SSG-EG (CH1, OP1)                  -   -   -   -  EN  INV ALT HOLD
\       $91: SSG-EG (CH2, OP1)
\       $92: SSG-EG (CH3, OP1)          EN: Enable.  1=active, 0=off
\ 
\       $94: SSG-EG (CH1, OP2)         INV: Invert.  Causes envelope generator
\       $95: SSG-EG (CH2, OP2)              to begin inverted.
\       $96: SSG-EG (CH3, OP2)
\                                      ALT: Alternate. Envelope generator 
\       $98: SSG-EG (CH1, OP3)              inverts after every cycle.
\       $99: SSG-EG (CH2, OP3)
\       $9A: SSG-EG (CH3, OP3)        HOLD: Envelope generator is held at max
\                                           or min after first cycle until
\       $9C: SSG-EG (CH1, OP4)              key off.
\       $9D: SSG-EG (CH2, OP4)
\       $9E: SSG-EG (CH3, OP4)
\                                               7   6   5   4   3   2   1   0
\       $A0: Frequency (LSBs) (CH1)            FREQ**************************
\       $A1: Frequency (LSBs) (CH2)
\       $A2: Frequency (LSBs) (CH3)
\                                               7   6   5   4   3   2   1   0
\       $A4: Frequency/Octave (CH1)             -   -  Octave****  FREQ******
\       $A5: Frequency/Octave (CH2)                 Octave = 0-7
\       $A6: Frequency/Octave (CH3)                 Freq   = 0-2047
\ 
\                                               7   6   5   4   3   2   1   0
\       $A8: CH3 Supp. Freq. (LSBs) (OP2)      FREQ**************************
\       $A9: CH3 Supp. Freq. (LSBs) (OP3)
\       $AA: CH3 Supp. Freq. (LSBs) (OP4)
\                                               7   6   5   4   3   2   1   0
\       $AC: CH3 Supp. Freq/Octave (OP2)        -   -  Octave****  FREQ******
\       $AD: CH3 Supp. Freq/Octave (OP3)
\       $AE: CH3 Supp. Freq/Octave (OP4)
\                                               7   6   5   4   3   2   1   0
\       $B0: Algorithm/Feedback (CH1)           -   -  FB********  ALG*******
\       $B1: Algorithm/Feedback (CH2)
\       $B2: Algorithm/Feedback (CH3)
\                                               7   6   5   4   3   2   1   0
\       $B4: Pan/LFO Sensitivity (CH1)          L   R  AMS***   -  FMS*******
\       $B5: Pan/LFO Sensitivity (CH2)
\       $B6: Pan/LFO Sensitivity (CH3)
\ 
( ---------------------------------------------------------------------------- )
\   FM Algorithms       []=carrier
\ 
\       _______________________________________________       
\       0:  1-2-3-[4]--> distortion guitar, high hat, bass
\       _______________________________________________       
\       1:  1_  
\            _3-[4]--> harp, PSG sound
\           2   
\       _______________________________________________       
\       2:    1_
\              _[4]--> bass, electric guitar, brass, piano, woods
\           2-3
\       _______________________________________________       
\       3:  1-2_
\              _[4]--> strings, folk guitar, chimes
\             3
\       _______________________________________________       
\       4:  1-[2]_
\                _--> flute, bells, chorus, bass drum, snare drum, tom-tom
\           3-[4] 
\       _______________________________________________       
\       5:     _[2]_
\             |     |
\           1-+-[3]-+--> brass, organ
\             |_   _|
\               [4]
\       _______________________________________________       
\       6:  1-[2]_
\                 |
\             [3]-+--> xylophone, tom-tom, organ, vibraphone
\                _|      snare drum, bass drum
\             [4]
\       _______________________________________________       
\       7:  [1]__ 
\                |
\           [2]--|
\                +--> pipe organ
\           [3]--|
\              __|
\           [4]  
\ 
( ---------------------------------------------------------------------------- )
\   PSG Communication
\                                       7   6   5   4   3   2   1   0
\       Latch Byte                      1   C****   T   Data*********
\           C: Channel (0-3)
\           T: Type (1=volume, 0=pitch)
\ 
\                                       7   6   5   4   3   2   1   0
\       Noise Generator Latch           1   1   1   0   0  FB  NF1 NF0
\           FB: Feedback (1=hiss, 0=periodic)
\           NF: 00 = higher pitch, less coarse
\               01 = medium
\               10 = lower pitch, more coarse
\               11 = tone generator #3
\ 
\                                       7   6   5   4   3   2   1   0
\       Frequency Data                  0   -   Data*****************
\ 
\ 
\       To adjust pitch, the Latch Byte contains the 4 LSBs of the frequency,
\         followed by a Data Byte with the 6 MSBs
\ 
\       To adjust volume, the Latch Byte contains all 4 bits of data
\ 
( ---------------------------------------------------------------------------- )





    




















