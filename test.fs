include system.fs
Forth definitions

:entry
    ['] (emit) is emit
    cr ." hello world!"
    
    cr ." write some numbers: " 12345 <# #s #> type space  $ABCDE hex.
    
    cr ." halting cpu..."
    begin again ;

romfile: test.gen




