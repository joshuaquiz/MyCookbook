--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.2

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: AuthorLinks; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AuthorLinks" (
    link_id uuid NOT NULL,
    author_id uuid NOT NULL,
    url text NOT NULL
);


--
-- Name: Authors; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Authors" (
    author_id uuid NOT NULL,
    name text NOT NULL,
    location text,
    bio text,
    is_visible boolean DEFAULT false NOT NULL,
    author_type integer DEFAULT 0 NOT NULL,
    email text,
    password_hash text,
    email_verified boolean DEFAULT false NOT NULL,
    provider_user_id text,
    auth_provider text DEFAULT 'local'::text NOT NULL,
    cognito_sub text,
    last_login_at timestamp without time zone,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


--
-- Name: Categories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Categories" (
    category_id uuid NOT NULL,
    category_name text NOT NULL
);


--
-- Name: EntityImages; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."EntityImages" (
    entity_image_id uuid NOT NULL,
    image_id uuid NOT NULL,
    entity_type integer DEFAULT 0 NOT NULL,
    entity_id uuid NOT NULL
);


--
-- Name: Images; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Images" (
    image_id uuid NOT NULL,
    url text NOT NULL,
    image_type integer DEFAULT 0 NOT NULL
);


--
-- Name: Ingredients; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Ingredients" (
    ingredient_id uuid NOT NULL,
    name text NOT NULL
);


--
-- Name: Popularity; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Popularity" (
    popularity_id uuid NOT NULL,
    entity_type integer DEFAULT 0 NOT NULL,
    entity_id uuid NOT NULL,
    metric_type integer DEFAULT 0 NOT NULL,
    count integer DEFAULT 0 NOT NULL
);


--
-- Name: RawDataSources; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RawDataSources" (
    source_id uuid NOT NULL,
    same_as uuid,
    url text NOT NULL,
    url_host text,
    processing_status integer DEFAULT 0 NOT NULL,
    page_type integer DEFAULT 0 NOT NULL,
    parser_version integer DEFAULT 0 NOT NULL,
    ld_json_data text,
    raw_html text,
    processed_datetime timestamp without time zone,
    error text
);


--
-- Name: RecipeCategories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RecipeCategories" (
    recipe_id uuid NOT NULL,
    category_id uuid NOT NULL
);


--
-- Name: RecipeHearts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RecipeHearts" (
    author_id uuid NOT NULL,
    recipe_id uuid NOT NULL,
    hearted_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


--
-- Name: RecipeStepIngredients; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RecipeStepIngredients" (
    step_ingredient_id uuid NOT NULL,
    recipe_step_id uuid NOT NULL,
    ingredient_id uuid NOT NULL,
    raw_text text NOT NULL,
    quantity_type integer DEFAULT 1,
    min_value numeric,
    max_value numeric,
    number_value numeric,
    measurement_type integer DEFAULT 0 NOT NULL,
    notes text
);


--
-- Name: RecipeSteps; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RecipeSteps" (
    step_id uuid NOT NULL,
    recipe_id uuid NOT NULL,
    step_number integer NOT NULL,
    step_type integer DEFAULT 0 NOT NULL,
    instructions text
);


--
-- Name: RecipeTags; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RecipeTags" (
    recipe_id uuid NOT NULL,
    tag_id uuid NOT NULL
);


--
-- Name: Recipes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Recipes" (
    recipe_id uuid NOT NULL,
    recipe_url_id uuid,
    author_id uuid NOT NULL,
    title text NOT NULL,
    type text NOT NULL,
    description text,
    prep_time integer,
    cook_time integer,
    servings text,
    rating numeric,
    is_public boolean DEFAULT true NOT NULL,
    original_recipe_id uuid,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


--
-- Name: ShoppingListItems; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ShoppingListItems" (
    shopping_list_item_id uuid NOT NULL,
    author_id uuid NOT NULL,
    ingredient_id uuid NOT NULL,
    recipe_step_id uuid NOT NULL,
    multiplier numeric DEFAULT 1.0 NOT NULL,
    notes text,
    is_purchased boolean DEFAULT false NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


--
-- Name: Tags; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Tags" (
    tag_id uuid NOT NULL,
    tag_name text NOT NULL
);


--
-- Name: UserCalendars; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserCalendars" (
    id uuid NOT NULL,
    author_id uuid NOT NULL,
    recipe_id uuid NOT NULL,
    date timestamp without time zone NOT NULL,
    meal_type integer DEFAULT 0 NOT NULL,
    servings_multiplier numeric DEFAULT 1.0 NOT NULL
);


--
-- Name: AuthorLinks AuthorLinks_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AuthorLinks"
    ADD CONSTRAINT "AuthorLinks_pkey" PRIMARY KEY (link_id);


--
-- Name: Authors Authors_cognito_sub_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Authors"
    ADD CONSTRAINT "Authors_cognito_sub_key" UNIQUE (cognito_sub);


--
-- Name: Authors Authors_email_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Authors"
    ADD CONSTRAINT "Authors_email_key" UNIQUE (email);


--
-- Name: Authors Authors_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Authors"
    ADD CONSTRAINT "Authors_pkey" PRIMARY KEY (author_id);


--
-- Name: Categories Categories_category_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Categories"
    ADD CONSTRAINT "Categories_category_name_key" UNIQUE (category_name);


--
-- Name: Categories Categories_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Categories"
    ADD CONSTRAINT "Categories_pkey" PRIMARY KEY (category_id);


--
-- Name: EntityImages EntityImages_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."EntityImages"
    ADD CONSTRAINT "EntityImages_pkey" PRIMARY KEY (entity_image_id);


--
-- Name: Images Images_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Images"
    ADD CONSTRAINT "Images_pkey" PRIMARY KEY (image_id);


--
-- Name: Ingredients Ingredients_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Ingredients"
    ADD CONSTRAINT "Ingredients_name_key" UNIQUE (name);


--
-- Name: Ingredients Ingredients_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Ingredients"
    ADD CONSTRAINT "Ingredients_pkey" PRIMARY KEY (ingredient_id);


--
-- Name: Popularity Popularity_entity_type_entity_id_metric_type_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Popularity"
    ADD CONSTRAINT "Popularity_entity_type_entity_id_metric_type_key" UNIQUE (entity_type, entity_id, metric_type);


--
-- Name: Popularity Popularity_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Popularity"
    ADD CONSTRAINT "Popularity_pkey" PRIMARY KEY (popularity_id);


--
-- Name: RawDataSources RawDataSources_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RawDataSources"
    ADD CONSTRAINT "RawDataSources_pkey" PRIMARY KEY (source_id);


--
-- Name: RawDataSources RawDataSources_url_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RawDataSources"
    ADD CONSTRAINT "RawDataSources_url_key" UNIQUE (url);


--
-- Name: RecipeCategories RecipeCategories_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeCategories"
    ADD CONSTRAINT "RecipeCategories_pkey" PRIMARY KEY (recipe_id, category_id);


--
-- Name: RecipeHearts RecipeHearts_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeHearts"
    ADD CONSTRAINT "RecipeHearts_pkey" PRIMARY KEY (author_id, recipe_id);


--
-- Name: RecipeStepIngredients RecipeStepIngredients_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeStepIngredients"
    ADD CONSTRAINT "RecipeStepIngredients_pkey" PRIMARY KEY (step_ingredient_id);


--
-- Name: RecipeSteps RecipeSteps_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeSteps"
    ADD CONSTRAINT "RecipeSteps_pkey" PRIMARY KEY (step_id);


--
-- Name: RecipeSteps RecipeSteps_recipe_id_step_number_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeSteps"
    ADD CONSTRAINT "RecipeSteps_recipe_id_step_number_key" UNIQUE (recipe_id, step_number);


--
-- Name: RecipeTags RecipeTags_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeTags"
    ADD CONSTRAINT "RecipeTags_pkey" PRIMARY KEY (recipe_id, tag_id);


--
-- Name: Recipes Recipes_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Recipes"
    ADD CONSTRAINT "Recipes_pkey" PRIMARY KEY (recipe_id);


--
-- Name: Recipes Recipes_recipe_url_id_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Recipes"
    ADD CONSTRAINT "Recipes_recipe_url_id_key" UNIQUE (recipe_url_id);


--
-- Name: ShoppingListItems ShoppingListItems_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ShoppingListItems"
    ADD CONSTRAINT "ShoppingListItems_pkey" PRIMARY KEY (shopping_list_item_id);


--
-- Name: Tags Tags_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Tags"
    ADD CONSTRAINT "Tags_pkey" PRIMARY KEY (tag_id);


--
-- Name: Tags Tags_tag_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Tags"
    ADD CONSTRAINT "Tags_tag_name_key" UNIQUE (tag_name);


--
-- Name: UserCalendars UserCalendars_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserCalendars"
    ADD CONSTRAINT "UserCalendars_pkey" PRIMARY KEY (id);


--
-- Name: AuthorLinks FK_AuthorLinks_Authors_author_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AuthorLinks"
    ADD CONSTRAINT "FK_AuthorLinks_Authors_author_id" FOREIGN KEY (author_id) REFERENCES public."Authors"(author_id);


--
-- Name: EntityImages FK_EntityImages_Images_image_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."EntityImages"
    ADD CONSTRAINT "FK_EntityImages_Images_image_id" FOREIGN KEY (image_id) REFERENCES public."Images"(image_id);


--
-- Name: RecipeCategories FK_RecipeCategories_Categories_category_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeCategories"
    ADD CONSTRAINT "FK_RecipeCategories_Categories_category_id" FOREIGN KEY (category_id) REFERENCES public."Categories"(category_id);


--
-- Name: RecipeCategories FK_RecipeCategories_Recipes_recipe_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeCategories"
    ADD CONSTRAINT "FK_RecipeCategories_Recipes_recipe_id" FOREIGN KEY (recipe_id) REFERENCES public."Recipes"(recipe_id);


--
-- Name: RecipeHearts FK_RecipeHearts_Authors_author_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeHearts"
    ADD CONSTRAINT "FK_RecipeHearts_Authors_author_id" FOREIGN KEY (author_id) REFERENCES public."Authors"(author_id);


--
-- Name: RecipeHearts FK_RecipeHearts_Recipes_recipe_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeHearts"
    ADD CONSTRAINT "FK_RecipeHearts_Recipes_recipe_id" FOREIGN KEY (recipe_id) REFERENCES public."Recipes"(recipe_id);


--
-- Name: RecipeStepIngredients FK_RecipeStepIngredients_Ingredients_ingredient_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeStepIngredients"
    ADD CONSTRAINT "FK_RecipeStepIngredients_Ingredients_ingredient_id" FOREIGN KEY (ingredient_id) REFERENCES public."Ingredients"(ingredient_id);


--
-- Name: RecipeStepIngredients FK_RecipeStepIngredients_RecipeSteps_recipe_step_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeStepIngredients"
    ADD CONSTRAINT "FK_RecipeStepIngredients_RecipeSteps_recipe_step_id" FOREIGN KEY (recipe_step_id) REFERENCES public."RecipeSteps"(step_id);


--
-- Name: RecipeSteps FK_RecipeSteps_Recipes_recipe_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeSteps"
    ADD CONSTRAINT "FK_RecipeSteps_Recipes_recipe_id" FOREIGN KEY (recipe_id) REFERENCES public."Recipes"(recipe_id);


--
-- Name: RecipeTags FK_RecipeTags_Recipes_recipe_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeTags"
    ADD CONSTRAINT "FK_RecipeTags_Recipes_recipe_id" FOREIGN KEY (recipe_id) REFERENCES public."Recipes"(recipe_id);


--
-- Name: RecipeTags FK_RecipeTags_Tags_tag_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RecipeTags"
    ADD CONSTRAINT "FK_RecipeTags_Tags_tag_id" FOREIGN KEY (tag_id) REFERENCES public."Tags"(tag_id);


--
-- Name: Recipes FK_Recipes_Authors_author_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Recipes"
    ADD CONSTRAINT "FK_Recipes_Authors_author_id" FOREIGN KEY (author_id) REFERENCES public."Authors"(author_id);


--
-- Name: Recipes FK_Recipes_RawDataSources_recipe_url_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Recipes"
    ADD CONSTRAINT "FK_Recipes_RawDataSources_recipe_url_id" FOREIGN KEY (recipe_url_id) REFERENCES public."RawDataSources"(source_id);


--
-- Name: ShoppingListItems FK_ShoppingListItems_Authors_author_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ShoppingListItems"
    ADD CONSTRAINT "FK_ShoppingListItems_Authors_author_id" FOREIGN KEY (author_id) REFERENCES public."Authors"(author_id);


--
-- Name: ShoppingListItems FK_ShoppingListItems_Ingredients_ingredient_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ShoppingListItems"
    ADD CONSTRAINT "FK_ShoppingListItems_Ingredients_ingredient_id" FOREIGN KEY (ingredient_id) REFERENCES public."Ingredients"(ingredient_id);


--
-- Name: UserCalendars FK_UserCalendars_Authors_author_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserCalendars"
    ADD CONSTRAINT "FK_UserCalendars_Authors_author_id" FOREIGN KEY (author_id) REFERENCES public."Authors"(author_id);


--
-- Name: UserCalendars FK_UserCalendars_Recipes_recipe_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserCalendars"
    ADD CONSTRAINT "FK_UserCalendars_Recipes_recipe_id" FOREIGN KEY (recipe_id) REFERENCES public."Recipes"(recipe_id);


--
-- PostgreSQL database dump complete
--

