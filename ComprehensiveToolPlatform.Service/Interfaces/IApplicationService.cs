using ComprehensiveToolPlatform.Repository.Models;

namespace ComprehensiveToolPlatform.Service
{
    public interface IApplicationService
    {
        HomeViewVo GetHomeData();

        Application GetApplicationById(string name);

        List<Category> GetCategories();

        void UpdateCategory(string id, CategoryUpdateDto dto);

        void DeleteCategory(string id);

        void AddCategory(CategoryCreateDto dto);

        void UpdateCategoriesSort(List<SortUpdateDto> sortUpdates);

        List<Application> GetApplications(string categoryId);

        void UpdateApplication(string id, ApplicationUpdateDto dto);

        void DeleteApplication(string id);

        void AddApplication(ApplicationCreateDto dto);

        void UpdateApplicationsSort(List<SortUpdateDto> sortUpdates);

        void UpdateApplicationsSortByCategory(string categoryId, List<SortUpdateDto> sortUpdates);
    }
}
