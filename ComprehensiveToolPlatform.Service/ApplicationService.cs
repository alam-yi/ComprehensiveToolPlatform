using ComprehensiveToolPlatform.Repository.Contexts;
using ComprehensiveToolPlatform.Repository.Models;

namespace ComprehensiveToolPlatform.Service
{
    public class ApplicationService : IApplicationService
    {
        private readonly EfCoreContext _context;

        public ApplicationService(EfCoreContext context)
        {
            _context = context;
        }


        public HomeViewVo GetHomeData()
        {
            var model = new HomeViewVo();

            // 获取所有激活的分类，按 Sort 排序
            var categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Sort)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .ToList();

            model.Categories = categories;

            // 获取所有激活的应用，按 Sort 排序
            var applications = _context.Applications
                .Where(a => a.IsActive)
                .OrderBy(a => a.Sort)
                .Select(a => new Application
                {
                    Id = a.Id,
                    CategoryId = a.CategoryId,
                    Name = a.Name,
                    Description = a.Description ?? string.Empty,
                    FileType = a.FileType,
                    FileSize = a.FileSize
                })
                .ToList();

            // 按 CategoryId 分组
            var appGroups = applications
                .GroupBy(a => a.CategoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            model.ApplicationsByCategory = appGroups;

            return model;
        }

        public Application GetApplicationById(string name)
        {
            return _context.Applications.FirstOrDefault(a => a.Name == name);
        }

        public List<Category> GetCategories()
        {
            return _context.Categories.OrderBy(c => c.Sort).ToList();
        }

        public void UpdateCategory(string id, CategoryUpdateDto dto)
        {
            var category = _context.Categories.FirstOrDefault(s => s.Id == id);
            if (category == null)
                throw new Exception("类别不存在，请刷新后重试");

            var exist = _context.Categories.Any(s => s.Name == dto.Name && s.Id != id);
            if (exist)
                throw new Exception("类别名已存在，请勿重复操作");

            category.Name = dto.Name;
            category.Sort = dto.Sort;
            category.IsActive = dto.IsActive;

            _context.Update(category);
            _context.SaveChanges();
        }

        public void DeleteCategory(string id)
        {
            var category = _context.Categories.FirstOrDefault(s => s.Id == id);
            if (category == null)
                throw new Exception("类别不存在，请刷新后重试");

            var applications = _context.Applications.Where(a => a.CategoryId == id).ToList();

            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Applications.RemoveRange(applications);
                    _context.Categories.Remove(category);
                    _context.SaveChanges();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw new Exception($"删除失败。{ex.Message}");
                }
            }
        }

        public void AddCategory(CategoryCreateDto dto)
        {
            var exist = _context.Categories.Any(s => s.Name == dto.Name);
            if (exist)
                throw new Exception("类别名已存在，请勿重复操作");

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Sort = dto.Sort,
                IsActive = true
            };

            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void UpdateCategoriesSort(List<SortUpdateDto> sortUpdates)
        {
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var update in sortUpdates)
                    {
                        var category = _context.Categories.FirstOrDefault(s => s.Id == update.Id);
                        if (category != null)
                        {
                            category.Sort = update.Sort;
                            _context.Update(category);
                        }
                    }
                    _context.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw new Exception($"类别排序失败。{ex.Message}");
                }
            }
        }

        public List<Application> GetApplications(string categoryId)
        {
            return _context.Applications.Where(s => s.CategoryId == categoryId).OrderBy(s => s.Sort).ToList();
        }

        public void UpdateApplication(string id, ApplicationUpdateDto dto)
        {
            var application = _context.Applications.FirstOrDefault(s => s.Id == id);
            if (application == null)
                throw new Exception("应用程序不存在，请刷新后重试");

            var exist = _context.Applications.Any(s => s.Name == dto.Name && s.Id != id);
            if (exist)
                throw new Exception("应用程序名已存在，请勿重复操作");

            application.Name = dto.Name;
            application.Description = dto.Description;
            application.Sort = dto.Sort;
            application.IsActive = dto.IsActive;
            application.CategoryId = dto.CategoryId;
            application.FileType = dto.FileType;
            application.FileSize = dto.FileSize;

            _context.Update(application);
            _context.SaveChanges();
        }

        public void DeleteApplication(string id)
        {
            var application = _context.Applications.FirstOrDefault(s => s.Id == id);
            if (application == null)
                throw new Exception("应用程序不存在，请刷新后重试");

            _context.Applications.Remove(application);
            _context.SaveChanges();
        }

        public void AddApplication(ApplicationCreateDto dto)
        {
            var exist = _context.Applications.Any(s => s.Name == dto.Name);
            if (exist)
                throw new Exception("应用程序名已存在，请勿重复操作");

            var application = new Application
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Description = dto.Description,
                Sort = dto.Sort,
                IsActive = dto.IsActive,
                CategoryId = dto.CategoryId,
                FileType = dto.FileType,
                FileSize = dto.FileSize
            };

            _context.Applications.Add(application);
            _context.SaveChanges();
        }

        public void UpdateApplicationsSort(List<SortUpdateDto> sortUpdates)
        {
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var update in sortUpdates)
                    {
                        var application = _context.Applications.FirstOrDefault(s => s.Id == update.Id);
                        if (application != null)
                        {
                            application.Sort = update.Sort;
                            _context.Update(application);
                        }
                    }

                    _context.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw new Exception($"应用程序排序失败。{ex.Message}");
                }
            }
        }

        public void UpdateApplicationsSortByCategory(string categoryId, List<SortUpdateDto> sortUpdates)
        {

            // 验证所有应用是否属于该类别
            var applicationIds = sortUpdates.Select(s => s.Id).ToList();
            var applicationsInCategory = _context.Applications
                .Where(a => a.CategoryId == categoryId && applicationIds.Contains(a.Id))
                .ToList();

            if (applicationsInCategory.Count != sortUpdates.Count)
                throw new Exception("部分应用不属于该类别");


            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var update in sortUpdates)
                    {
                        var application = applicationsInCategory.FirstOrDefault(a => a.Id == update.Id);
                        if (application != null)
                        {
                            application.Sort = update.Sort;
                            _context.Update(application);
                        }
                    }

                    _context.SaveChanges();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw new Exception($"应用程序排序失败。{ex.Message}");
                }
            }
        }
    }
}
