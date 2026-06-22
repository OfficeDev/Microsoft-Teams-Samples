// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

package com.microsoft.bot.sample.teamsstartnewthread;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.junit4.SpringRunner;

/**
 * This class contains unit tests for the Spring Boot application context.
 * 
 * It is used to verify that the Spring application context can load successfully 
 * without errors or issues during startup. It ensures that all the necessary 
 * Spring beans and components for the bot application are properly initialized.
 * 
 * The contextLoads() method is a simple placeholder test to verify the application context.
 */
@RunWith(SpringRunner.class) // Tells JUnit to run with SpringRunner to enable Spring Boot test support.
@SpringBootTest // Loads the full application context to verify the setup of the Spring Boot application.
public class ApplicationTest {

    /**
     * Test method to verify that the Spring application context loads successfully.
     * This test will pass if the application context is loaded without issues.
     * 
     * @throws Exception if any error occurs while loading the application context.
     */
    @Test
    public void contextLoads() {
        // No action needed. If the application context fails to load, an exception will be thrown.
    }

}
