# Use a smaller base image for the build stage
FROM gradle:4.7.0-jdk8-alpine AS build

# Copy only the necessary files required for building
COPY --chown=gradle:gradle build.gradle settings.gradle gradlew /home/gradle/src/
COPY --chown=gradle:gradle gradle /home/gradle/src/gradle
COPY --chown=gradle:gradle src /home/gradle/src/src

# Set the working directory
WORKDIR /home/gradle/src

# Build the application
RUN ./gradlew build --no-daemon

# Use a smaller base image for the final stage
FROM openjdk:8-jre-slim

# Set the working directory
WORKDIR /app

# Expose port
EXPOSE 8080

# Copy the JAR file from the build stage
COPY --from=build /home/gradle/src/build/libs/*.jar /app/spring-boot-application.jar

# Set the entrypoint
ENTRYPOINT ["java", "-XX:+UnlockExperimentalVMOptions", "-XX:+UseCGroupMemoryLimitForHeap", "-Djava.security.egd=file:/dev/./urandom", "-jar", "/app/spring-boot-application.jar"]
