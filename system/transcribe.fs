( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(           Words to be preserved in the host machine's environment            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       TODO:                                                                  )
(           - implement 's>number?' so we don't have to rely on gForth's       )
(                                                                              )
( ---------------------------------------------------------------------------- )

                                transcription

                0=  0<>  =  <>  >  >=  <  <=  u>  u>=  u<  u<=
    >r  r@  r>  2>r  2r@  2r>  @  !  +!  c@  c!  2@  2!  l!  w!  l@  w@
            +  -  1+  1-  *  m*  /  mod  /mod  */  */mod  2*  2/
        <#  #  #s  #>  :  '  .  d.  u.  ud.  .r  d.r  u.r  ud.r  .s  ]
    abort  abs  align  aligned  allocate  allot  and  base  bin  bl  >body  bye
            cell+  cells  char  close-file  cmove  cmove>  compare
            constant  2constant  count  cr  create  create-file
            d>s  decimal  depth  drop  2drop  dup  2dup  ?dup
        emit  execute  [else]  false  find-name  free  here  hex  hold
                i  [if]  immediate  include  invert  j  k
            lshift  max  min  move  negate  nip  2nip  :noname
            off  on  or  over  2over  page  parse  parse-name  pick
    r/o  r/w  refill  roll  rot  2rot  rshift  s>d  space  spaces  swap  2swap
    [then]  throw  true  tuck  2tuck  type  value  2value  variable  2variable
                        w/o  within  write-file  xor

                            s>number?  s>unumber?

                               -transcription

( ---------------------------------------------------------------------------- )
(       Immediate Words                                                        )
( ---------------------------------------------------------------------------- )
: ;       no-int postpone ;       ; immediate
: if      no-int postpone if      ; immediate
: else    no-int postpone else    ; immediate
: then    no-int postpone then    ; immediate
: endif   no-int postpone then    ; immediate
: exit    no-int postpone exit    ; immediate
: begin   no-int postpone begin   ; immediate
: again   no-int postpone again   ; immediate
: until   no-int postpone until   ; immediate
: while   no-int postpone while   ; immediate
: repeat  no-int postpone repeat  ; immediate
:  do     no-int postpone  do     ; immediate
: ?do     no-int postpone ?do     ; immediate
:  loop   no-int postpone  loop   ; immediate
: +loop   no-int postpone +loop   ; immediate
: unloop  no-int postpone unloop  ; immediate
: leave   no-int postpone leave   ; immediate
: case    no-int postpone case    ; immediate
: of      no-int postpone of      ; immediate
: endof   no-int postpone endof   ; immediate
: endcase no-int postpone endcase ; immediate
: does>   no-int postpone does>   ; immediate
: ."      no-int postpone ."      ; immediate
: to      no-int postpone to      ; immediate
: [       no-int postpone [       ; immediate
: [char]  no-int postpone [char]  ; immediate
: abort"  no-int postpone abort"  ; immediate

: literal   no-int postpone literal  ; immediate
: 2literal  no-int postpone 2literal ; immediate
: postpone  no-int postpone postpone ; immediate

: s" state @ if postpone s" exit endif [char] " parse ; immediate

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

























