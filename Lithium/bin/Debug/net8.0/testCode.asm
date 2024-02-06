global _start
_start:
    push 44
    push 17
    push 42
    push 193
    push 193
    jmp l1
l0:
    push QWORD [rsp + 40] ;; 7
    push QWORD [rsp + 40] ;; 7
    pop rdi
    pop rax
    add rax, rdi
    push rax
    push QWORD [rsp + 32] ;; 7
    pop rdi
    pop rax
    add rax, rdi
    push rax
    push QWORD [rsp + 16] ;; 7
    pop rdi
    pop rax
    sub rax, rdi
    push rax
    push QWORD [rsp + 0] ;; 7
    pop rdi
    mov rax, 60
    syscall
    add rsp, 8
l1:
    mov rdi, 4 ;; 3
    mov [rsp + 8], rdi ;; 1
    mov rdi, 7 ;; 3
    mov [rsp + 0], rdi ;; 1
    call l0
    add rsp, 16
    push QWORD [rsp + 24] ;; 7
    pop rdi
    mov rax, 60
    syscall
