# Basic Jwt Bearer Event Demo Implementation

### 1. How the JWT token validation works in ASP.NET Core application?

#### Explained:

- The key lies in the relationship between the token generation and validation setup.

#### How its works:

- First, in the `DependencyInjection.cs`, configured the authentication middleware with specific JWT validation parameters:

```csharp
services.AddAuthentication(...)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });
```

- Then in the `JwtTokenGenerator` generate tokens using the same parameters:

```csharp
var token = new JwtSecurityToken(
    issuer: _jwtSettings.Issuer,
    audience: _jwtSettings.Audience,
    expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.ExpiredMinutes),
    claims: claims,
    signingCredentials: signingCredentials
);
```

### 2. How does the validation work?

**Shared Secret -** Both the generator and validator use the same secret key (jwtSettings.Secret). This is crucial because:

* The **generator** uses it to sign the token
* The **validator** uses it to verify the signature

**Matching Parameters -** The generator and validator use the same:

* **Issuer** (jwtSettings.Issuer)
* **Audience** (jwtSettings.Audience)
* **Signing algorithm** (HmacSha256)

### 3. When a request comes in with a JWT token:

- The authentication middleware intercepts the request.
- It extracts the JWT token from the Authorization header.
- It validates the token using the configured TokenValidationParameters.
- If the token is valid (correct signature, not expired, valid issuer/audience), the request proceeds.
- If invalid, it triggers one of your error handlers (OnAuthenticationFailed, OnChallenge, or OnForbidden).

* For example, when curl to TestAuth endpoint:

```csharp
[HttpGet("test-auth")]
[Authorize]
public IActionResult TestAuth()
{
    return Ok("Authentication access successfully.");
}
```

**Code explained:**

* The `[Authorize]` attribute triggers the authentication middleware to validate the token using the same secret and parameters used to generate it. This shared configuration is what allows the system to verify that tokens are genuine and haven't been tampered with.

* The security comes from the fact that only your server knows the secret key used to sign and validate the tokens. Even if someone intercepts a token, they can't modify it without knowing the secret key, as this would invalidate the signature.

### 4. What if I visit the JWT website to modify my JWT role? For instance, if I change my role from "user" to "admin" and then paste it back into the authorized bearer token, would that make me an admin?

- JWT cannot successfully modify like that, even using tools like jwt.io. Here's why:

A JWT token consists of three parts, separated by dots:

* Header
* Payload (where claims like roles are stored)
* Signature

The critical part is the signature. When you generate a token in your JwtTokenGenerator, this happens:

```csharp
var signingCredentials = new SigningCredentials(
    new SymmetricSecurityKey(secret), 
    SecurityAlgorithms.HmacSha256
);
```

This creates a cryptographic signature using your server's secret key. If you try to modify the payload (like changing a role), the signature will no longer match because:

* You can decode and modify the payload on jwt.io.
* But you cannot generate a valid new signature because you don't have the server's secret key.
* When the modified token reaches your server, this validation will fail:

```csharp
options.TokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings.Secret)
    )
    // ...
};
```

The middleware will detect that the signature doesn't match the modified payload and reject the token, triggering your OnAuthenticationFailed handler:

```csharp
OnAuthenticationFailed = context =>
{
    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    result = JsonConvert.SerializeObject(new { message = "Internal server error." });
    return context.Response.WriteAsync(result);
}
```

This is why it's crucial to:

* Keep your server's secret key secure.
* Always validate tokens on the server side.
* Use strong secret keys (long, random strings).

The only way to get a valid admin token would be to either:

* Have legitimate admin access granted through your application.
* Somehow have your server's secret key (which would be a serious security breach).

**Note:** This is also why you should never store sensitive information in a JWT payload—while it can be decoded and read, **the signature mechanism prevents it from being tampered with**.

# Reference:

- [ASP.NET Authentication: JWT-Bearer events](https://medium.com/@diver.stv/asp-net-authentication-jwt-bearer-events-8efcbe615596)
- [JwtBearerEvents in ASP.NET Core: Managing Token Events](https://medium.com/@sametkarademir244/jwtbearerevents-in-asp-net-core-managing-token-events-04cdeb9dc30d)
- [Jwt bearer event asp.net core](https://www.google.com/search?q=jwt+bearer+event+asp.net+core&oq=jwt+bearer+event+asp.net+core&gs_lcrp=EgZjaHJvbWUyBggAEEUYOTIGCAEQRRhA0gEINjA2OGowajGoAgCwAgA&sourceid=chrome&ie=UTF-8)
