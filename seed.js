const mongoose = require('mongoose');
const bcrypt = require('bcrypt');
const Recipe = require('./models/recipeModel');
const Category = require('./models/categoryModel');
const User = require('./models/userModel');

// Data to be seeded
const categories = [
  { name: 'Appetizers' },
  { name: 'Main Course' },
  { name: 'Desserts' },
  { name: 'Beverages' },
  { name: 'Salads' },
  { name: 'Soups' },
  { name: 'Breakfast' },
  { name: 'Snacks' },
  { name: 'Side Dishes' },
  { name: 'Vegan' }
];

const users = [
  {
    firstname: "John",
    lastname: "Doe",
    email: "john.doe@example.com",
    password: "password123",
  },
  {
    firstname: "Jane",
    lastname: "Smith",
    email: "jane.smith@example.com",
    password: "password123",
  },
  {
    firstname: "Alice",
    lastname: "Johnson",
    email: "alice.johnson@example.com",
    password: "password123",
  }
];

let userMap;
let categoryMap;

function getTestUsersMap(){
    return userMap;
}

function getTestCategoryMap() {
    return categoryMap;
}

const seedData = async () => {
  try {
    await Recipe.deleteMany({});
    await Category.deleteMany({});
    await User.deleteMany({});
    console.log('Collections cleared');

    const insertedCategories = await Category.insertMany(categories);
    console.log('Categories seeded successfully');

    categoryMap = insertedCategories.reduce((map, Category) => {
      map[Category.name] = Category._id;
      return map;
    }, {});

    for (let user of users) {
      user.password = await hashPassword(user.password);
    }
    const insertedUsers = await User.insertMany(users);
    console.log('Users seeded successfully');

    userMap = insertedUsers.reduce((map, user) => {
      map[user.email] = user._id;
      return map;
    }, {});

    const recipes = [
      {
        title: 'Spaghetti Carbonara',
        description: 'A classic Italian pasta dish made with eggs, cheese, pancetta, and pepper.',
        ingredients: [
          { name: 'Spaghetti', quantity: '200g' },
          { name: 'Pancetta', quantity: '100g' },
          { name: 'Eggs', quantity: '2 large' },
          { name: 'Parmesan Cheese', quantity: '50g' },
          { name: 'Black Pepper', quantity: 'to taste' },
          { name: 'Salt', quantity: 'to taste' }
        ],
        instructions: [
          { step: 'Cook the spaghetti according to package instructions.' },
          { step: 'In a pan, cook the pancetta until crispy.' },
          { step: 'Beat the eggs and mix with grated Parmesan cheese.' },
          { step: 'Drain the spaghetti and combine with pancetta, then remove from heat.' },
          { step: 'Quickly mix in the egg and cheese mixture, stirring constantly to create a creamy sauce.' },
          { step: 'Season with black pepper and salt to taste.' }
        ],
        cookingTime: 20,
        servings: 2,
        category: categoryMap["Main Course"]
      },
      {
        title: 'Chicken Curry',
        description: 'A flavorful and spicy chicken curry with a rich, creamy sauce.',
        ingredients: [
          { name: 'Chicken Breast', quantity: '500g' },
          { name: 'Onion', quantity: '1 large, chopped' },
          { name: 'Garlic', quantity: '3 cloves, minced' },
          { name: 'Ginger', quantity: '1 inch, grated' },
          { name: 'Coconut Milk', quantity: '400ml' },
          { name: 'Curry Powder', quantity: '2 tbsp' },
          { name: 'Tomato Paste', quantity: '2 tbsp' },
          { name: 'Vegetable Oil', quantity: '2 tbsp' },
          { name: 'Salt', quantity: 'to taste' },
          { name: 'Cilantro', quantity: 'for garnish' }
        ],
        instructions: [
          { step: 'Heat oil in a pot and sauté onions until translucent.' },
          { step: 'Add garlic and ginger, cooking until fragrant.' },
          { step: 'Stir in curry powder and tomato paste, cooking for another 2 minutes.' },
          { step: 'Add chicken pieces and cook until no longer pink.' },
          { step: 'Pour in coconut milk, bring to a boil, then simmer for 20 minutes.' },
          { step: 'Season with salt and garnish with chopped cilantro before serving.' }
        ],
        cookingTime: 40,
        servings: 4,
        category: categoryMap["Main Course"]
      },
      {
        title: 'Chocolate Chip Cookies',
        description: 'Crispy on the outside, chewy on the inside, these cookies are a timeless classic.',
        ingredients: [
          { name: 'Butter', quantity: '200g' },
          { name: 'Sugar', quantity: '100g' },
          { name: 'Brown Sugar', quantity: '150g' },
          { name: 'Eggs', quantity: '2 large' },
          { name: 'Vanilla Extract', quantity: '1 tsp' },
          { name: 'Flour', quantity: '300g' },
          { name: 'Baking Soda', quantity: '1 tsp' },
          { name: 'Salt', quantity: '1/2 tsp' },
          { name: 'Chocolate Chips', quantity: '200g' }
        ],
        instructions: [
          { step: 'Preheat oven to 180°C (350°F).' },
          { step: 'Cream together butter, sugar, and brown sugar until smooth.' },
          { step: 'Beat in the eggs one at a time, then stir in the vanilla extract.' },
          { step: 'Combine flour, baking soda, and salt; gradually blend into the creamed mixture.' },
          { step: 'Stir in chocolate chips.' },
          { step: 'Drop by rounded spoonfuls onto ungreased cookie sheets.' },
          { step: 'Bake for 8 to 10 minutes, or until edges are nicely browned.' }
        ],
        cookingTime: 25,
        servings: 24,
        category: categoryMap["Desserts"]
      }
    ];

    await Recipe.insertMany(recipes);
    console.log('Recipes seeded successfully');
  } catch (error) {
    console.error('Error seeding data:', error);
  }
};

const hashPassword = async (password) => {
  const salt = await bcrypt.genSalt(10);
  return await bcrypt.hash(password, salt);
};

module.exports = {
  seedData,
  getTestUsersMap,
  getTestCategoryMap
}
