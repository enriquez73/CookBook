import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Media, RecipeDetail, RecipeFilter, RecipeRequest, RecipeStep, RecipeSummary } from './models';

@Injectable({ providedIn: 'root' })
export class RecipeService {
  private readonly http = inject(HttpClient);

  list(filter: RecipeFilter): Observable<RecipeSummary[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<RecipeSummary[]>('/api/recipes', { params });
  }

  get(id: string): Observable<RecipeDetail> {
    return this.http.get<RecipeDetail>(`/api/recipes/${id}`);
  }

  create(body: RecipeRequest): Observable<RecipeDetail> {
    return this.http.post<RecipeDetail>('/api/recipes', body);
  }

  update(id: string, body: RecipeRequest): Observable<RecipeDetail> {
    return this.http.put<RecipeDetail>(`/api/recipes/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`/api/recipes/${id}`);
  }

  tags(): Observable<string[]> {
    return this.http.get<string[]>('/api/tags');
  }

  uploadMedia(id: string, file: File, caption?: string): Observable<Media> {
    const form = new FormData();
    form.append('file', file);
    if (caption) form.append('caption', caption);
    return this.http.post<Media>(`/api/recipes/${id}/media`, form);
  }

  addVideoUrl(id: string, url: string, caption?: string): Observable<Media> {
    return this.http.post<Media>(`/api/recipes/${id}/media/video-url`, { url, caption });
  }

  addImageUrl(id: string, url: string, caption?: string): Observable<Media> {
    return this.http.post<Media>(`/api/recipes/${id}/media/image-url`, { url, caption });
  }

  deleteMedia(id: string, mediaId: string): Observable<void> {
    return this.http.delete<void>(`/api/recipes/${id}/media/${mediaId}`);
  }

  uploadStepImage(id: string, stepId: string, file: File): Observable<RecipeStep> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<RecipeStep>(`/api/recipes/${id}/steps/${stepId}/image`, form);
  }

  addStepImageUrl(id: string, stepId: string, url: string): Observable<RecipeStep> {
    return this.http.post<RecipeStep>(`/api/recipes/${id}/steps/${stepId}/image-url`, { url, caption: null });
  }

  deleteStepImage(id: string, stepId: string): Observable<void> {
    return this.http.delete<void>(`/api/recipes/${id}/steps/${stepId}/image`);
  }
}
