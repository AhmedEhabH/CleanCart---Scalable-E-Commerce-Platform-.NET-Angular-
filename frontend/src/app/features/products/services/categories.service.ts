import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Category, CategorySimple } from '../models/category.model';
import { ApiResponse } from '../../../core/models';

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(`${this.baseUrl}/categories`);
  }

  getCategoryOptions(): Observable<CategorySimple[]> {
    return new Observable<CategorySimple[]>(observer => {
      this.getCategories().subscribe({
        next: (response) => {
          if (response.data) {
            const flat = this.flattenCategories(response.data);
            observer.next(flat);
            observer.complete();
          } else {
            observer.next([]);
            observer.complete();
          }
        },
        error: (err) => {
          observer.error(err);
        }
      });
    });
  }

  private flattenCategories(categories: Category[], result: CategorySimple[] = []): CategorySimple[] {
    for (const category of categories) {
      result.push({ id: category.id, name: category.name });
      if (category.children && category.children.length > 0) {
        this.flattenCategories(category.children, result);
      }
    }
    return result;
  }
}
