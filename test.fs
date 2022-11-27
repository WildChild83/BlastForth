include system.fs
Forth definitions

: maybethrow ( -- )
    cr ." will I throw?"
    0 throw
    cr ." didn't throw"
;

:entry

    ['] (emit) is emit

    ." hello world!"
    
    ['] maybethrow catch cr ." throw code: " <# #s #> type
    
    cr ." top of ram: " $FFFFFC @ hex.
    cr ." middle:     " $FF8000 @ hex.
    
    cr ." halting cpu..."
    
    begin again ;

romfile: test.gen




