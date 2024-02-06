global _start
_start:
    push 98
    push 7
    push QWORD [rsp + 8] ;; 7
    push QWORD [rsp + 8] ;; 7
    pop rdi
    pop rax
    sub rax, rdi
    push rax
    pop rdi
    mov [rsp + 8], rdi ;; 4
    push QWORD [rsp + 8] ;; 7
    pop rdi
    mov rax, 60
    syscall
