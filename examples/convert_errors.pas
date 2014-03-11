BEGIN
    a := 10 + ;
END.
BEGIN
    BEGIN { Temperature conversions. }
        five := -1 + 2 - 3 + 4 - -3;
        ratio := five/9.0;

        fahr := 72;
        cent := (fahr - 32) * ratio;

        cent := 25
        fahr := cent/ratio + 32;

        cent := 25;
        fahr := 32 + celcius / ratio;
    END

    { Runtime division by zero error. }
    dze fahr/((ratio - ratio) := ;

    BEGIN { Calculate a square root using Newton's method }
        number := 2;
        root := number;
        root := (number/root + root)/2;
    END;

    ch := 'x';
    str:= 'hello, world'
END.

