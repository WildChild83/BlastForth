include system.fs
Forth definitions

variable cycles

: nothing ( -- ) 
    #frames 30 > if
        \ 0 to #frames
        \ text-color-index halves dup read-cram hvdp> 2 +
        \ swap write-cram h>vdp
        \ 1 cycles +!
    endif ;

record
    field: .x
    field: .y
    field: .z
constant struc

code do-something ( -- n ) tos push, $BABEFACE # tos move, next

: entry
    ['] nothing is vertical-blank-handler
    cycles off
    
    $0080 backcolor
    
    interrupts  +vbi !video
    
    cr ." hello world!"
    
    cr cr ." pad: " pad hex.
    
    cr cr ." node ptr: " (node) nextnode .node
    
    cr cr ." 100 alloc-find: " 
    cr 100 (alloc-find) . ?lastnode .flag hex. hex.
    
    cr cr terminal-xy
        begin 2dup at-xy ." #frames: " #frames . cycles @ 8 = until
    
    

    ;    

romfile: test.gen






















