using Azure;
using CommonRepositories;
using FilmsServices.Converters.Common;
using FilmsServices.Validators.Common;
using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Services.Common
{
    public class BaseSevice<DB, VM> : IService<VM>
        where DB : BaseEntity
        where VM : BaseViewModel
    {
        private readonly IRepository<DB> repository;
        protected readonly IEntityConverter<DB, VM> converter;
        protected readonly IValidator<VM> validator;

        public BaseSevice(
            IRepository<DB> repository,
            IEntityConverter<DB, VM> converter,
            IValidator<VM> validator)
        {
            this.repository = repository;
            this.converter = converter;
            this.validator = validator;
        }




        public virtual async Task<List<VM>> GetAllAsync(int limit, int page)
        {
            List<DB> fromDB = await repository.GetAllAsync(limit, page);
            return fromDB
                .Select(d => converter.ConvertToVm(d))
                .ToList();
        }

        public virtual async Task<List<VM>> GetAllAsync()
        {
            List<DB> fromDB = await repository.GetAllAsync();
            return fromDB
                .Select(d => converter.ConvertToVm(d))
                .ToList();
        }

        public virtual async Task<VM> GetByIdAsync(long id)
        {
            DB fromDB = await repository.GetByIdAsync(id);
            return converter.ConvertToVm(fromDB);
        }

        public virtual async Task<int> GetCountAsync()
        {
            return await repository.GetCountAsync();
        }



        public virtual async Task<ValidationResult> AddAsync(VM entity)
        {
            ValidationResult vr = validator.Validate(entity);

            if (vr.Success)
            {
                DB toDb = converter.ConvertToDb(entity);
                await repository.AddAsync(toDb);
            }

            return vr;
        }

        public virtual async Task<ValidationResult> UpdateAsync(VM entity)
        {
            ValidationResult vr = validator.Validate(entity);

            if (vr.Success)
            {
                DB toDb = converter.ConvertToDb(entity);
                await repository.UpdateAsync(toDb);
            }

            return vr;
        }

        public virtual async Task<ValidationResult> UpdateAllAsync(List<VM> entities)
        {
            ValidationResult vr = new ValidationResult(true);
            foreach (VM entity in entities)
            {
                vr.AddResult(validator.Validate(entity));
            }

            if (vr.Success)
            {
                List<DB> toDb = entities
                    .Select(e => converter.ConvertToDb(e))
                    .ToList();

                await repository.UpdateAllAsync(toDb);
            }

            return vr;
        }



        public virtual async Task DeleteAsync(long id)
        {
            await repository.DeleteAsync(id);
        }
    }
}
