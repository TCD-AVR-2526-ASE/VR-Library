FROM eclipse-temurin:17-jre
WORKDIR /app

ENV SPRING_PROFILES_ACTIVE=test

COPY library-boot/target/library-boot-*.jar app.jar


EXPOSE 6201

ENTRYPOINT ["java", "-Djava.security.egd=file:/dev/./urandom", "-Duser.timezone=Europe/Dublin", "-Xms256m", "-Xmx512m", "-jar", "app.jar"]