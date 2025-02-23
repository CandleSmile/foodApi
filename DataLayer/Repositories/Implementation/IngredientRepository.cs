﻿using DataLayer.Items;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementation
{
    public class IngredientRepository : BaseRepository<Ingredient>, IIngredientRepository
    {
        public IngredientRepository(FoodContext context) : base(context)
        {
        }

        public async Task<Ingredient?> GetIngredientByNameAsync(string name)
        {
            return await _context.Ingredients.FirstOrDefaultAsync(ing => ing.Name == name);
        }
    }
}
