const express = require("express");
const {
  createRecipe,
  getaRecipe,
  getAllRecipes,
  updateRecipe,
  deleteRecipe
} = require("../controller/recipeCtrl");
const { authMiddleware } = require("../middlewares/authMiddleware");
const router = express.Router();

router.get("/", getAllRecipes);
router.get("/:id", getaRecipe);
router.post("/", authMiddleware, createRecipe);
router.put("/:id", authMiddleware, updateRecipe);
router.delete("/:id", authMiddleware, deleteRecipe);

module.exports = router;