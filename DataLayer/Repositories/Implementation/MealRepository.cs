﻿using DataLayer.Items;
using DataLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories.Implementation
{
    public class MealRepository : BaseRepository<Meal>, IMealRepository
    {
        public MealRepository(FoodContext context) : base(context)
        {
        }

        public async Task<Meal?> GetMealByIdWithInfoAsync(int id)
        {
            return await _context.Meals.Include(meal => meal.Tags).Include(meal => meal.Area).Include(meal => meal.Category).Include(meal => meal.Ingredients).FirstOrDefaultAsync(meal => meal.Id == id);
        }


        public async Task<Meal?> GetMealByNameAsync(string name)
        {
            return await _context.Meals.FirstOrDefaultAsync(meal => meal.Name == name);
        }

        public async Task<IEnumerable<Meal>?> GetMealsAsync(string? searchString, int? idCategory, IEnumerable<int>? ingredientsIds)
        {
            var countIngredients = ingredientsIds != null ? ingredientsIds.Count() : 0;
            IQueryable<Meal> query = _context.Meals;
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(meal => meal.Name.Contains(searchString));
            }

            if (idCategory != null && idCategory > 0)
            {
                query = query.Where(meal => meal.CategoryId == idCategory);
            }

            if (countIngredients > 0)
            {
                var queryIngredients = ingredientsIds?.AsQueryable();
                var queryListMealsWithIngredients = this._context.Meals.Include(meal => meal.Ingredients).SelectMany(meal => meal.Ingredients, (meal, ing) =>
                            new
                            {
                                MealId = meal.Id,
                                IngredientId = ing.Id,
                            })
                         .Where(item => ingredientsIds.Contains(item.IngredientId))
                         .GroupBy(item => new { item.MealId })
                         .Select(g => new { Id = g.Key.MealId, count = g.Count() })
                         .Where(item => item.count == countIngredients)
                         .Select(item => item.Id);
                query = query.Where(meal => queryListMealsWithIngredients.Contains(meal.Id));
            }

            return await query.Include(meal => meal.Tags).Include(meal => meal.Area).Include(meal => meal.Category).ToListAsync();
        }

        public async Task<IEnumerable<Meal>?> GetMealsAsync(int count, int skip)
        {
            return await _context.Meals.Skip(skip).Take(count).Include(meal => meal.Tags).Include(meal => meal.Area).Include(meal => meal.Category).ToListAsync();
        }
    }
}
