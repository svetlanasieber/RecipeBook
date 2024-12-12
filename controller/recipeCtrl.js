const Recipe = require("../models/recipeModel");
const asyncHandler = require("express-async-handler");
const slugify = require("slugify");
const validateMongoDbId = require("../utils/validateMongodbId");

const createRecipe = asyncHandler(async (req, res) => {
  try {
    if (req.body.title) {
      req.body.slug = slugify(req.body.title);
    }
    
    let newRecipe = await Recipe.create(req.body);
    newRecipe = await newRecipe.populate('category');
    res.json(newRecipe);
  } catch (error) {
    throw new Error(error);
  }
});

const updateRecipe = asyncHandler(async (req, res) => {
  const { id } = req.params; 
  validateMongoDbId(id); 

  try {
    if (req.body.title) {
      req.body.slug = slugify(req.body.title); 
    }

    let updatedRecipe = await Recipe.findByIdAndUpdate(id, req.body, {
      new: true,
      runValidators: true
    });
    updatedRecipe = await updatedRecipe.populate('category');
    res.json(updatedRecipe);
  } catch (error) {
    throw new Error(error);
  }
});

const deleteRecipe = asyncHandler(async (req, res) => {
  const { id } = req.params; 
  validateMongoDbId(id); 

  try {
    let deletedRecipe = await Recipe.findByIdAndDelete(id);
    deletedRecipe = await deletedRecipe.populate('category');
    res.json(deletedRecipe);
  } catch (error) {
    throw new Error(error);
  }
});


const getaRecipe = asyncHandler(async (req, res) => {
  const { id } = req.params;
  validateMongoDbId(id);
  try {
    const findRecipe = await Recipe.findById(id).populate('category');
    res.json(findRecipe);
  } catch (error) {
    throw new Error(error);
  }
});

const getAllRecipes = asyncHandler(async (req, res) => {
  try {
    const queryObj = { ...req.query };
    const excludeFields = ["page", "sort", "limit", "fields"];
    excludeFields.forEach((el) => delete queryObj[el]);
    let queryStr = JSON.stringify(queryObj);
    queryStr = queryStr.replace(/\b(gte|gt|lte|lt)\b/g, (match) => `$${match}`);

    let query = Recipe.find(JSON.parse(queryStr)).populate('category');

    if (req.query.sort) {
      const sortBy = req.query.sort.split(",").join(" ");
      query = query.sort(sortBy);
    } else {
      query = query.sort("-createdAt");
    }

    if (req.query.fields) {
      const fields = req.query.fields.split(",").join(" ");
      query = query.select(fields);
    } else {
      query = query.select("-__v");
    }

    const page = req.query.page;
    const limit = req.query.limit;
    const skip = (page - 1) * limit;
    query = query.skip(skip).limit(limit);
    if (req.query.page) {
      const RecipesCount = await Recipe.countDocuments();
      if (skip >= RecipesCount) throw new Error("This Page does not exists");
    }
    const recipe = await query;
    res.json(recipe);
  } catch (error) {
    throw new Error(error);
  }
});

module.exports = {
  createRecipe,
  getaRecipe,
  getAllRecipes,
  updateRecipe,
  deleteRecipe
};
