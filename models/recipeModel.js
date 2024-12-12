const mongoose = require('mongoose');
const idValidator = require('mongoose-id-validator');

const recipeSchema = new mongoose.Schema(
  {
    title: {
      type: String,
      required: true,
      trim: true
    },
    description: {
      type: String,
      trim: true
    },
    ingredients: [
      {
        name: {
          type: String,
          required: true
        },
        quantity: {
          type: String,
          required: true
        }
      }
    ],
    instructions: [
      {
        step: {
          type: String,
          required: true
        }
      }
    ],
    cookingTime: {
      type: Number, // time in minutes
      required: true
    },
    servings: {
      type: Number,
      required: true
    },
    category: {
      type: mongoose.Schema.Types.ObjectId, 
      ref: "Category",
      required: true
    }
  },
  {
    toObject: {
      transform: function (doc, ret) {
        delete ret.__v;
      }
    },
    toJSON: {
      transform: function (doc, ret) {
        delete ret.__v;
      }
    },
    timestamps: true
  }
);

recipeSchema.plugin(idValidator);
module.exports = mongoose.model('Recipe', recipeSchema);
