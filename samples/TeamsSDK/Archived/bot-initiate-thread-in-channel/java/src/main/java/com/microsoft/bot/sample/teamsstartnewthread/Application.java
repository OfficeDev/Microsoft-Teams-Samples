// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

package com.microsoft.bot.sample.teamsstartnewthread;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Import;

import com.microsoft.bot.builder.Bot;
import com.microsoft.bot.integration.AdapterWithErrorHandler;
import com.microsoft.bot.integration.BotFrameworkHttpAdapter;
import com.microsoft.bot.integration.Configuration;
import com.microsoft.bot.integration.spring.BotController;
import com.microsoft.bot.integration.spring.BotDependencyConfiguration;

/**
 * Entry point for the Spring Boot Bot application.
 * This class provides default Bot configurations and overrides methods for custom implementations.
 */
@SpringBootApplication
// Use the default BotController to receive incoming Channel messages. A custom
// controller could be used by eliminating this import and creating a new
// org.springframework.web.bind.annotation.RestController.
// The default controller is created by the Spring Boot container using
// dependency injection. The default route is /api/messages.
@Import({BotController.class}) // Import default BotController for handling incoming messages

/**
 * This class extends the BotDependencyConfiguration which provides the default
 * implementations for a Bot application.  The Application class should
 * override methods in order to provide custom implementations.
 */
public class Application extends BotDependencyConfiguration {

    public static void main(String[] args) {
        SpringApplication.run(Application.class, args);
    }

    /**
     * Returns the Bot for this application.
     * <p>
     *     The @Component annotation could be used on the Bot class instead of this method
     *     with the @Bean annotation.
     * </p>
     * @return The Bot implementation for this application.
     */
    @Bean
    public Bot getBot(@Autowired Configuration configuration) {
        // TeamsStartNewThreadBot is assumed to be a @Component and managed by Spring
        return new TeamsStartNewThreadBot(configuration);
    }

    /**
     * Returns a custom Adapter with error handling for the bot.
     * 
     * @param configuration The Configuration object to use.
     * @return A BotFrameworkHttpAdapter with error handling.
     */
    @Override
    public BotFrameworkHttpAdapter getBotFrameworkHttpAdaptor(Configuration configuration) {
        return new AdapterWithErrorHandler(configuration);
    }
}
