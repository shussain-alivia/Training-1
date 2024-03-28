# Use the official OpenJDK 11 image as base image
FROM openjdk:11-jre-slim

# Set the working directory inside the container
WORKDIR /app

# Copy the Gradle files needed for dependency resolution
COPY build.gradle .
COPY settings.gradle .
COPY gradlew .

# Copy the entire source code
COPY src src

# Copy the Gradle wrapper files (if using the Gradle Wrapper)
COPY gradle gradle

# Build the application using Gradle
RUN ./gradlew build

# Copy the built JAR file to a specific directory within the container
COPY build/libs/your-application.jar /app/your-application.jar

# Expose the port your application listens on
EXPOSE 8080

# Command to run the application
CMD ["java", "-jar", "your-application.jar"]
