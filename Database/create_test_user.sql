--
-- Create test user for API testing
-- Username: testuser@mycookbook.com
-- Password: TestPassword123!
-- Password Hash: SHA256 hash of "TestPassword123!"
--

INSERT INTO public."Authors" (
    author_id,
    name,
    location,
    bio,
    is_visible,
    author_type,
    email,
    password_hash,
    email_verified,
    provider_user_id,
    auth_provider,
    cognito_sub,
    last_login_at,
    created_at
) VALUES (
    'a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d'::uuid,
    'Test User',
    'Test City, Test Country',
    'This is a test user account for API testing purposes.',
    true,
    1, -- AuthorType.User
    'testuser@mycookbook.com',
    '/8EhoiEJWL905ah0Zo89l40ktqgkFJbM/zwOokXk8SY=', -- SHA256 hash of "TestPassword123!"
    true,
    NULL,
    'local',
    NULL,
    NULL,
    CURRENT_TIMESTAMP
)
ON CONFLICT (author_id) DO NOTHING;

-- Note: The password for this test user is: TestPassword123!
-- The hash is generated using SHA256 algorithm.

