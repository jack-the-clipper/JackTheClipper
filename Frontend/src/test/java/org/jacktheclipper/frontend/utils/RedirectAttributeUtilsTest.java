package org.jacktheclipper.frontend.utils;

import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;
import org.springframework.web.servlet.mvc.support.RedirectAttributesModelMap;

import java.util.Map;

public class RedirectAttributeUtilsTest {

    private RedirectAttributes redirectAttributes;

    @Before
    public void init() {

        redirectAttributes = new RedirectAttributesModelMap();
    }

    @Test
    public void populateDefaultWithError() {

        RedirectAttributesUtils.populateDefaultRedirectAttributes(redirectAttributes, true, "This"
                + " is a test");
        Map<String, ?> map = redirectAttributes.getFlashAttributes();
        Assert.assertEquals("This is a test", map.get("msg"));
        Assert.assertEquals("alert-warning", map.get("css"));
    }

    @Test
    public void populateDefaultNoError() {

        RedirectAttributesUtils.populateDefaultRedirectAttributes(redirectAttributes, false,
                "This is a test");

        Map<String, ?> map = redirectAttributes.getFlashAttributes();
        Assert.assertEquals("This is a test", map.get("msg"));
        Assert.assertEquals("alert-info", map.get("css"));
    }
}
