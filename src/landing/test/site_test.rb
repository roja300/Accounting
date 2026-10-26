require "minitest/autorun"
require "pathname"
require "yaml"

class LandingSiteTest < Minitest::Test
  ROOT = Pathname.new(__dir__).join("..").expand_path
  SOURCE_PATTERNS = ["*.{md,html}", "_layouts/**/*.{md,html}", "_includes/**/*.{md,html}"].freeze

  def site_source_content
    SOURCE_PATTERNS
      .flat_map { |pattern| Dir[ROOT.join(pattern).to_s] }
      .uniq
      .sort
      .map { |path| File.read(path) }
      .join("\n")
  end

  def test_required_markdown_pages_have_front_matter
    %w[index.md getting-started.md guide.md developers.md 404.md].each do |relative_path|
      content = ROOT.join(relative_path).read
      assert_match(/\A---\r?\n/, content, "#{relative_path} must start with Jekyll front matter")
      assert_operator content.lines.count { |line| line.chomp == "---" }, :>=, 2, "#{relative_path} must close its front matter"
    end
  end

  def test_internal_page_targets_exist
    routes = site_source_content.scan(/\{\{\s*['\"](\/[a-z0-9\-\/]*\/?)['\"]\s*\|\s*relative_url\s*\}\}/).flatten.uniq
    routes.reject { |route| route.start_with?("/assets/") || route == "/" }.each do |route|
      expected = ROOT.join("#{route.delete_prefix("/").delete_suffix("/")}.md")
      assert expected.file?, "#{route} should resolve to a Markdown page"
    end
  end

  def test_referenced_assets_exist
    assets = site_source_content.scan(/['\"](\/assets\/[a-zA-Z0-9._\-\/]+)['\"]/).flatten.uniq
    refute_empty assets
    assets.each do |asset|
      source = ROOT.join(asset.delete_prefix("/"))
      source = source.sub_ext(".scss") if source.extname == ".css" && !source.file?
      assert source.file?, "#{asset} should have a source file"
    end
  end

  def test_site_source_scan_excludes_non_site_html
    refute_includes site_source_content, "non-site dependency fixture"
  end

  def test_jekyll_excludes_cached_dependencies
    config = YAML.safe_load(ROOT.join("_config.yml").read)

    assert_includes config.fetch("exclude"), "vendor"
  end

  def test_site_links_to_the_source_repository
    config = ROOT.join("_config.yml").read
    assert_includes config, "github_url: \"https://github.com/roja300/Accounting\""
    assert_includes ROOT.join("index.md").read, "site.github_url"
  end

  def test_repository_readme_points_to_published_documentation
    readme = ROOT.join("../..", "README.md").read

    assert_includes readme, "https://roja300.github.io/Accounting/"
    assert_includes readme, "https://roja300.github.io/Accounting/developers/"
  end

  def test_production_startup_is_documented
    readme = ROOT.join("../..", "README.md").read
    getting_started = ROOT.join("getting-started.md").read

    assert_includes readme, "./start-production.ps1"
    assert_includes readme, "src/.env.production.example"
    assert_includes getting_started, "./start-production.ps1"
    assert_includes getting_started, "src/compose.production.yaml"
    assert_includes getting_started, "-NoBrowser"
    assert_includes getting_started, "docker compose --env-file .env.development up --build"
  end
end
