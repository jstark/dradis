BEGIN
    BEGIN { Temperature conversions. }
        five := -1 + 2 - 3 + 4 + 3;
        ratio := five/9.0;

        fahr := 72;
        cent := (fahr - 32) * ratio;

        cent := 25;
        fahr := cent/ratio + 32;

        cent := 25;
        fahr := 32 + cent / ratio;
    END;

    { Runtime division by zero error. }
    dze := fahr/(ratio - ratio);

    BEGIN { Calculate a square root using Newton's method }
        number := 2;
        root := number;
        root := (number/root + root)/2;
        root := (number/root + root)/2;
        root := (number/root + root)/2;
        root := (number/root + root)/2;
        root := (number/root + root)/2;
    END;

    BEGIN { relational operators }
	t1 := 1 < 10;
	t2 := 2 >= 0;
	t3 := 2.1 > 1;
	t4 := 1 > 2 && 3 < 10;
	t5 := t4
	t6 := t5 + 1;
    END;

    ch := 'x';
    str:= 'hello, world'
END.

