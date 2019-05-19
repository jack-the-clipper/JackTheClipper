package org.jacktheclipper.frontend.enums;

import org.junit.Assert;
import org.junit.Test;

public class NotificationSettingTest {

    @Test
    public void testNoneToString() {

        Assert.assertEquals("Keine Benachrichtigungen", NotificationSetting.None.toString());
    }

    @Test
    public void testMailToString() {

        Assert.assertEquals("Mail", NotificationSetting.LinkPerMail.toString());
    }

    @Test
    public void testPdfWithMailToString() {

        Assert.assertEquals("Mail mit PDF", NotificationSetting.PdfPerMail.toString());
    }
}
