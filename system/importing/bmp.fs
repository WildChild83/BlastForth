( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       BMP File Loader                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - import.fs                                                        )
(                                                                              )
( ---------------------------------------------------------------------------- )
Importer definitions {

( ---------------------------------------------------------------------------- )
: ?bmp ( addr u -- addr u flag )
    dup 14 <= if false exit endif
    over 2 + dword@ over = third word@ $4D42 = and ;

( ---------------------------------------------------------------------------- )
variable pixel-array        variable color-table
false value top-down?       0 value #colors

: load-bmp-24 ( addr u -- addr u )
    top-down? if
        pixel-array @   rgb-image   width @ height @ * 2*   bounds
        ?do  bgr@+ i rgb!  2 +loop  drop  exit endif
    width @ 2*   pixel-array @
    rgb-image  height @ fourth * fourth - over + ?do
        i third bounds ?do bgr@+ i rgb! 2 +loop
        over negate +loop 2drop ;

: load-bmp ( addr u -- addr u )
    cr over 14 + dword@ . ." byte header"
    bpp @ case
        24 of load-bmp-24 endof
        true abort" Only 24bpp implemented so far!"
    endcase ;

( ---------------------------------------------------------------------------- )
: bmp12header ( addr u -- addr u )
    over 22 + word@ 1 <> abort" BMP with >1 color plane is not supported."
    over 18 + word@ width !     over 20 + word@ height !
    over 24 + word@ bpp !       over 26 + color-table !
    0 to #colors   false to top-down? ;
: bmp16header ( addr u -- addr u )
    over 26 + word@ 1 <> abort" BMP with >1 color plane is not supported."
    over 18 + sdword@ width !
    false to top-down?
    over 22 + sdword@ dup 0< if negate true to top-down? endif height !
    over 28 + word@ bpp !
    over 30 + color-table !   0 to #colors ;
: bmp40header ( addr u -- addr u )
    bmp16header
    over 30 + dword@ 0<> abort" Compressed BMPs not yet supported."
    over 46 + dword@ to #colors
    over 54 + color-table ! ;
: bmp64header  ( addr u -- addr u ) bmp40header over  80 + color-table ! ;
: bmp108header ( addr u -- addr u ) bmp40header over 108 + color-table ! ;
: bmp124header ( addr u -- addr u ) bmp40header over 124 + color-table ! ;
: init-bmp ( addr u -- addr u )
    over 10 + dword@ third + pixel-array !
    over 14 + dword@ case
         12 of  bmp12header endof
         40 of  bmp40header endof
         64 of  bmp64header endof
        108 of bmp108header endof
        124 of bmp124header endof
        true abort" Unrecognized .BMP file header."
    endcase
    color-table @ pixel-array @ = if color-table off endif
    color-table @ #colors 0= and if 1 bpp @ lshift to #colors endif 
    ['] load-bmp is load-image ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )














