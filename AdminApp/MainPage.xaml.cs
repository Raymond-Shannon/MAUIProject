using Firebase.Storage;
using Google.Cloud.Firestore;
using System.Text.Json;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;
using Google.Cloud.Firestore.V1;

namespace AdminApp
{
		public partial class MainPage : ContentPage
		{
				public static FirebaseStorage firebaseStorage;
				public static string bucket_name = "voicedata-a5fa1.appspot.com";
				public static FirestoreDb firestoreDb { get; private set; }
				public List<String> users = new List<String>();
				List<String> user_names = new List<String>();

				public List<String> files;

				public MainPage()
				{
						InitializeComponent();


						firebaseStorage = new FirebaseStorage(bucket_name);
						firestoreDb = FirestoreDb.Create("voicedata-a5fa1");

						Initialize();


				}

				private async void Initialize()
				{
						var firebaseHelper = new FirebaseStorageHelper(bucket_name);

						files = await firebaseHelper.ListAllWavFilesRecursivelyAsync();

						foreach (var file in files)
						{
								string user_id = file.Split('/')[0];
								if(users.IndexOf(user_id) == -1)
								{
										users.Add(user_id);
								}
						}

						foreach (var user in users) {
								UserModel user_model = await GetUserById(user);

								user_names.Add(user_model.Email);
						}

						MyListView.ItemsSource = user_names;
				}

				public async Task<UserModel> GetUserById(string userId)
				{
						try
						{
								DocumentReference docRef = firestoreDb.Collection("users").Document(userId);
								DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
								Dictionary<string, object> userData = snapshot.ToDictionary();

								if (snapshot.Exists)
								{
										return new UserModel
										{
												Email = userData.TryGetValue("email", out object email) ? email as string : null,
												IsFirstLogin = userData.TryGetValue("isFirstLogin", out object isFirstLogin) && (bool)isFirstLogin,
												Token = userData.TryGetValue("token", out object token) ? token as string : null,
												Username = userData.TryGetValue("username", out object username) ? username as string : null,
												CreatedAt = userData.TryGetValue("createdAt", out object createdAt) ? ((Timestamp)createdAt).ToDateTime() : DateTime.MinValue
										};
								}
								else
								{
										Console.WriteLine($"No user found with ID: {userId}");
										return null;
								}
						}
						catch (Exception ex)
						{
								Console.WriteLine($"Error getting user: {ex.Message}");
								return null;
						}
				}

				public async void OnItemTapped(object sender, ItemTappedEventArgs e)
				{
						if (MyListView.SelectedItem != null)
						{
								List<String> user_files= new List<String>();
								string user_id = users[user_names.IndexOf(MyListView.SelectedItem.ToString())];

								foreach(var file in files)
								{
										if(file.StartsWith(user_id))
										{
												user_files.Add(file.Split('/').Last());
										}
								}

								FilesView.ItemsSource = user_files;
						}
				}

				private async void OnPlayButtonClicked(String filename)
				{
						try
						{
								// Get the download URL of your .wav file
								var audioUrl = await firebaseStorage
										.Child(filename)
										.GetDownloadUrlAsync();

								// Set the source of the MediaElement
								mediaElement.Source = MediaSource.FromUri(audioUrl);

								// Play the audio
								mediaElement.Play();
						}
						catch (Exception ex)
						{
								await DisplayAlert("Error", $"Could not play audio: {ex.Message}", "OK");
						}
				}

				public async Task<string> DownloadFileAsync(string path, string filename)
				{
						try
						{
								// Get a reference to the file in Firebase Storage
								var storage = new FirebaseStorage("voicedata-a5fa1.appspot.com");
								var fileReference = storage.Child(path);

								// Get the download URL
								var downloadUrl = await fileReference.GetDownloadUrlAsync();

								// Download the file
								using (var httpClient = new HttpClient())
								{
										var fileBytes = await httpClient.GetByteArrayAsync(downloadUrl);

										// Save the file to local storage
										string localPath = Path.Combine(FileSystem.CacheDirectory, filename);
										File.WriteAllBytes(localPath, fileBytes);

										return localPath;
								}
						}
						catch (Exception ex)
						{
								Console.WriteLine($"Error downloading file: {ex.Message}");
								return null;
						}
				}

				private void FilesView_ItemTapped(object sender, ItemTappedEventArgs e)
				{
						string filename = "";
						foreach (var file in files)
						{
								if (file.EndsWith(FilesView.SelectedItem.ToString()))
								{
										filename = file;
								}
						}
						OnPlayButtonClicked(filename);
				}

				private void StopAndReleaseAudio()
				{
						if (mediaElement?.CurrentState == MediaElementState.Playing)
						{
								mediaElement.Stop();
						}
						mediaElement?.Handler?.DisconnectHandler();
				}

				private void ContentPage_Disappearing(object sender, EventArgs e)
				{
						base.OnDisappearing();
						StopAndReleaseAudio();

						mediaElement?.Handler?.DisconnectHandler();
						mediaElement = null;
				}
		}

		public class FirebaseStorageHelper
		{
				private readonly string _bucketName;
				private readonly HttpClient _httpClient;

				public FirebaseStorageHelper(string bucketName)
				{
						_bucketName = bucketName;
						_httpClient = new HttpClient();
				}

				public async Task<List<string>> ListAllWavFilesRecursivelyAsync(string basePath = "")
				{
						var allWavFiles = new List<string>();
						await ListWavFilesRecursively(basePath, allWavFiles);
						return allWavFiles;
				}

				private async Task ListWavFilesRecursively(string path, List<string> fileList)
				{
						try
						{
								string url = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o?prefix={Uri.EscapeDataString(path)}";
								string nextPageToken = null;

								do
								{
										var requestUrl = string.IsNullOrEmpty(nextPageToken) ? url : $"{url}&pageToken={nextPageToken}";

										// Send a GET request to the Firebase Storage API
										var response = await _httpClient.GetStringAsync(requestUrl);

										// Parse the JSON response
										var jsonDocument = JsonDocument.Parse(response);

										// Check if the "items" array is available and iterate over it to extract file names
										if (jsonDocument.RootElement.TryGetProperty("items", out var items))
										{
												foreach (var item in items.EnumerateArray())
												{
														string fileName = item.GetProperty("name").GetString();

														// Filter to include only .wav files
														if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
														{
																fileList.Add(fileName);
														}
												}
										}

										// Check for a nextPageToken and set it for the next iteration
										nextPageToken = jsonDocument.RootElement.TryGetProperty("nextPageToken", out var tokenElement)
												? tokenElement.GetString()
												: null;

								} while (!string.IsNullOrEmpty(nextPageToken));
						}
						catch (Exception ex)
						{
								Console.WriteLine($"Error listing files in {path}: {ex.Message}");
						}
				}
		}

}
