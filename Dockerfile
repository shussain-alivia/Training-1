# Stage 1: Build the application
FROM gradle:4.7.0-jdk8-alpine AS build

# Set the working directory inside the container
WORKDIR /home/gradle/src

# Copy only the necessary files for building the application
COPY build.gradle settings.gradle /home/gradle/src/
COPY gradle /home/gradle/src/gradle
COPY src /home/gradle/src/src

# Make the Gradle Wrapper script executable
RUN chmod +x /home/gradle/src/gradlew

# Build the application using Gradle
RUN ./gradlew build --no-daemon

# Stage 2: Create the final lightweight image
FROM openjdk:8-jre-slim

# Set the working directory inside the container
WORKDIR /app

# Copy the built JAR file from the previous stage
COPY --from=build /home/gradle/src/build/libs/*.jar /app/application.jar

# Expose the port your application listens on
EXPOSE 8080

# Command to run the application
CMD ["java", "-jar", "application.jar"]
