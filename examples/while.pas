BEGIN { WHILE-DO statements. }
    i := 0; j := 0;

    WHILE i > j DO k := i;

    BEGIN { Calculate the a square root using Newton's method. }
        number := 4;
        root := number;

        WHILE root * root - number > 0.000001 DO BEGIN
             root := (number/root + root)/2
        END
    END;
END.

