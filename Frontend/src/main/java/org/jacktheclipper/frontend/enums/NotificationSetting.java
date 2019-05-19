package org.jacktheclipper.frontend.enums;

/**
 * The different kinds of notifications a user of the newsclipper can choose from
 */
public enum NotificationSetting {
    PdfPerMail,
    LinkPerMail,
    None;

    /**
     * Makes the values of this enum human readable.
     * {@link #name()} yields the name of the enum constants which is first of all english in an
     * otherwise completely german application and second not conform to the specification.
     *
     * @return A human readable interpretation
     */
    @Override
    public String toString() {

        String returnee = null;
        switch (this) {

            case PdfPerMail:
                returnee = "Mail mit PDF";
                break;
            case LinkPerMail:
                returnee = "Mail";
                break;
            case None:
                returnee = "Keine Benachrichtigungen";
                break;
        }
        return returnee;
    }
}