

Filter description.

Filter tries spam keywords(lsSpam_db.xml) against top lines of message.
Each keyword has cost, what a higher value, then more spam suspected.

If total cost > 99, message is considered as spam.

Example.

/// Message

Just open link and get FREE PRON and VIAGRA ...
There are aso many SEX movies ..

// end of message

Spam db
    KeyWord      Cost
    FREE PORN    50
    VIAGRA       50
    SEX          20
    .....


With this filter maximum allowed cost is exceeded and message is rejected.
 


