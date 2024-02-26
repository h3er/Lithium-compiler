global _start
_start:
    push 1
    push 1
    jmp l1
l0:
    push QWORD [rsp + 16]
    push QWORD [rsp + 16]
    pop rdi
    pop rax
    add rax, rdi
    push rax
    pop rdi
    mov rax, 60
    syscall
    add rsp, 0
l1:
    push 1
    push 1
    jmp l3
l2:
    push QWORD [rsp + 16]
    push QWORD [rsp + 16]
    pop rdi
    pop rax
    cmp rdi, rax
    jg l4
    push QWORD [rsp + 16]
    push QWORD [rsp + 16]
    pop rdi
    pop rax
    sub rax, rdi
    push rax
    pop rdi
    mov rax, 60
    syscall
    add rsp, 0
    jmp l5
l4:
    push QWORD [rsp + 8]
    push QWORD [rsp + 24]
    pop rdi
    pop rax
    sub rax, rdi
    push rax
    pop rdi
    mov rax, 60
    syscall
    jmp l5
l5:
    add rsp, 0
l3:
    add rsp, 0
    mov rdi, 80
    mov [rsp + 24], rdi
    mov rdi, 4
    mov [rsp + 16], rdi
    call l0
    add rsp, 16
    push 99
    pop rdi
    mov rax, 60
    syscall
